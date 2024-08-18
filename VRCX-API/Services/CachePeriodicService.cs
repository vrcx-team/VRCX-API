namespace VRCX_API.Services
{
    public class CachePeriodicService : BackgroundService
    {
        private readonly ILogger<CachePeriodicService> _logger;
        private readonly GithubCacheService _githubCacheService;
        private readonly CloudflareService _cloudflareService;
        private DateTime _lastRefresh = DateTime.MinValue;

        public CachePeriodicService(ILogger<CachePeriodicService> logger, GithubCacheService githubCacheService, CloudflareService cloudflareService)
        {
            _logger = logger;
            _githubCacheService = githubCacheService;
            _cloudflareService = cloudflareService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _githubCacheService.RefreshAsync();
            _lastRefresh = DateTime.Now;
            await _cloudflareService.PurgeCache();
            TriggerGC();

            using PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromSeconds(60));
            while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    if (DateTime.Now - _lastRefresh > TimeSpan.FromSeconds(120))
                    {
                        var hasChanged = await _githubCacheService.RefreshAsync();
                        _lastRefresh = DateTime.Now;

                        if(hasChanged)
                        {
                            await _cloudflareService.PurgeCache();
                        }

                        TriggerGC();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to refresh github cache");
                }
            }
        }

        private static void TriggerGC()
        {
            GC.Collect(2, GCCollectionMode.Aggressive);
            GC.WaitForFullGCComplete();
        }
    }
}
