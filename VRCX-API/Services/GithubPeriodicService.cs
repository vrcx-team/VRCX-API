namespace VRCX_API.Services
{
    public class GithubPeriodicService : BackgroundService
    {
        private readonly ILogger<GithubPeriodicService> _logger;
        private readonly GithubCacheService _githubCacheService;
        private DateTime _lastRefresh = DateTime.MinValue;

        public GithubPeriodicService(ILogger<GithubPeriodicService> logger, GithubCacheService githubCacheService)
        {
            _logger = logger;
            _githubCacheService = githubCacheService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _githubCacheService.RefreshAsync();
            _lastRefresh = DateTime.Now;

            using PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromSeconds(60));
            while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    if (DateTime.Now - _lastRefresh > TimeSpan.FromSeconds(120))
                    {
                        await _githubCacheService.RefreshAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to refresh github cache");
                }
            }
        }
    }
}
