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
        private readonly VrChatStatusCacheService _vrChatStatusCacheService;

        private DateTime _githubLastRefresh = DateTime.MinValue;
        private DateTime _vrChatStatusLastRefresh = DateTime.MinValue;

        public CachePeriodicService(ILogger<CachePeriodicService> logger, CloudflareService cloudflareService, GithubCacheService githubCacheService, VrChatStatusCacheService vrChatStatusCacheService)
        {
            _logger = logger;
            _cloudflareService = cloudflareService;
            _githubCacheService = githubCacheService;
            _vrChatStatusCacheService = vrChatStatusCacheService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _githubCacheService.RefreshAsync();
            _githubLastRefresh = DateTime.Now;

            await _vrChatStatusCacheService.RefreshAsync();
            _vrChatStatusLastRefresh = DateTime.Now;

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

                    if (DateTime.Now - _vrChatStatusLastRefresh > TimeSpan.FromSeconds(VrChatStatusRefreshInterval))
                    {
                        hasSomethingRefreshed |= true;

                        // We will not be caching the status.
                        // hasSomethingChanged |= await _vrChatStatusCacheService.RefreshAsync();
                        await _vrChatStatusCacheService.RefreshAsync();

                        _vrChatStatusLastRefresh = DateTime.Now;
                    }

                    if (hasSomethingChanged)
                    {
                        await _cloudflareService.PurgeCache();
                    }

                    if (hasSomethingRefreshed)
                    {
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
