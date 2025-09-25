using System.Collections.Frozen;
using VRCX_API.Configs;

namespace VRCX_API.Services
{
    public class VrChatStatusCacheService
    {
        public IReadOnlyDictionary<string, string> Graphs => _graphs;
        public IReadOnlyDictionary<string, string> Statuses => _statuses;

        private readonly ILogger<VrChatStatusCacheService> _logger;

        private readonly HttpClient _httpClient;

        private const string GraphsBaseUrl = "https://d31qqo63tn8lj0.cloudfront.net/";
        public static readonly FrozenSet<string> GraphPaths = FrozenSet.Create(
            "visits.json",
            "apilatency.json",
            "apirequests.json",
            "apierrors.json",
            "extauth_steam.json",
            "extauth_oculus.json"
        );

        private const string VrChatStatusBaseUrl = "https://status.vrchat.com/";
        public static readonly FrozenSet<string> VrChatStatusPaths = FrozenSet.Create(
            "api/v2/summary.json",
            "api/v2/status.json",
            "api/v2/components.json",
            "api/v2/incidents.json",
            "api/v2/incidents/unresolved.json",
            "api/v2/scheduled-maintenances.json",
            "api/v2/scheduled-maintenances/active.json",
            "api/v2/scheduled-maintenances/upcoming.json"
        );

        private Dictionary<string, string> _graphs = GraphPaths.ToDictionary(g => g, g => string.Empty);
        private Dictionary<string, string> _statuses = VrChatStatusPaths.ToDictionary(p => p, p => string.Empty);

        public VrChatStatusCacheService(ILogger<VrChatStatusCacheService> logger)
        {
            _logger = logger;
            _httpClient = new();

            // add authorization header
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(VrcxConfig.Config.Instance.UserAgent);
        }

        public async Task<bool> RefreshAsync()
        {
            bool hasChanged = false;

            hasChanged |= await RefreshGraphs();
            hasChanged |= await RefreshStatus();

            return hasChanged;
        }

        private async Task<bool> RefreshGraphs()
        {
            bool hasChanged = false;

            foreach (string path in GraphPaths)
            {
                try
                {
                    using HttpResponseMessage response = await _httpClient.GetAsync(GraphsBaseUrl + path);
                    string content = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        if (_graphs[path] != content)
                        {
                            _graphs[path] = content;
                            hasChanged = true;
                        }
                    }
                    else
                    {
                        _logger.LogError("Error fetching graph: {Path}, StatusCode: {StatusCode}, Content: {Content}", path, response.StatusCode, content);
                    }

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fetching graph: {Path}", path);
                }
            }

            return hasChanged;
        }

        private async Task<bool> RefreshStatus()
        {
            bool hasChanged = false;

            foreach (string path in VrChatStatusPaths)
            {
                try
                {
                    using HttpResponseMessage response = await _httpClient.GetAsync(VrChatStatusBaseUrl + path);
                    string content = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        if (_statuses[path] != content)
                        {
                            _statuses[path] = content;
                            hasChanged = true;
                        }
                    }
                    else
                    {
                        _logger.LogError("Error fetching status: {Path}, StatusCode: {StatusCode}, Content: {Content}", path, response.StatusCode, content);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fetching status: {Path}", path);
                }
            }

            return hasChanged;
        }

    }
}
