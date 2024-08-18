using CloudFlare.Client;
using CloudFlare.Client.Api.Authentication;
using VRCX_API.Configs;

namespace VRCX_API.Services
{
    public class CloudflareService : IHostedService
    {
        private readonly ILogger<CloudflareService> _logger;
        private readonly CloudFlareClient _client;
        private string _zoneId = string.Empty;

        public CloudflareService(ILogger<CloudflareService> logger)
        {
            _logger = logger;

            var authentication = new ApiTokenAuthentication(CommonConfig.Config.Instance.CloudflareAPIKey);
            _client = new CloudFlareClient(authentication);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var zones = await _client.Zones.GetAsync();

            if (!zones.Success)
            {
                throw new Exception("Failed to get zones from Cloudflare");
            }

            var vrcxZone = zones.Result.FirstOrDefault(x => x.Name == "vrcx.app");

            if (string.IsNullOrWhiteSpace(vrcxZone?.Id))
            {
                throw new Exception("Failed to get zone id for vrcx.app");
            }

            _zoneId = vrcxZone.Id;
        }

        public async Task PurgeCache()
        {
            var result = await _client.Zones.PurgeAllFilesAsync(_zoneId, true);
            if (!result.Success)
            {
                throw new Exception("Failed to purge cache for vrcx.app");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
