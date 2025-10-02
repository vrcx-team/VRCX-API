namespace VRCX_API.Services
{
    public class CachePeriodicService : BackgroundService
    {
        private const int PeriodicTimerInterval = 20;
        private const int GithubRefreshInterval = 120;
        private const int VrChatStatusRefreshInterval = 30;

        private readonly ILogger<CachePeriodicService> _logger;
        private readonly CloudflareService _cloudflareService;
        private readonly GithubCacheService _githubCacheService;

        private DateTime _githubLastRefresh = DateTime.MinValue;

        public CachePeriodicService(ILogger<CachePeriodicService> logger, CloudflareService cloudflareService, GithubCacheService githubCacheService)
        {
            _logger = logger;
            _cloudflareService = cloudflareService;
            _githubCacheService = githubCacheService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _githubCacheService.RefreshAsync();
            _githubLastRefresh = DateTime.Now;

            await _cloudflareService.PurgeCache();
            TriggerGC();

            using PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromSeconds(PeriodicTimerInterval));
            while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    var hasSomethingRefreshed = false;
                    var hasSomethingChanged = false;

                    if (DateTime.Now - _githubLastRefresh > TimeSpan.FromSeconds(GithubRefreshInterval))
                    {
                        hasSomethingRefreshed |= true;
                        hasSomethingChanged |= await _githubCacheService.RefreshAsync();
                        _githubLastRefresh = DateTime.Now;
                    }

                    if (hasSomethingChanged)
                    {
                        await _cloudflareService.PurgeCache();
                    }

                    /* API is no longer running a low-memory environment. So we don't need to trigger GC anymore.
                    if (hasSomethingRefreshed)
                    {
                        TriggerGC();
                    }
                    */
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
