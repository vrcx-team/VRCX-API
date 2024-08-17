using System.Text.Json;
using System.Text.Json.Serialization;
using GitHub;
using GitHub.Octokit.Client;
using GitHub.Octokit.Client.Authentication;
using VRCX_API.Configs;
using VRCX_API.Helpers;

namespace VRCX_API.Services
{
    public class GithubCacheService
    {
        public IReadOnlyCollection<GitHub.Models.Release> StableReleases => _stableReleases;
        public IReadOnlyCollection<GitHub.Models.Release> NightlyReleases => _nighltyReleases;
        public IReadOnlyCollection<GitHub.Models.RepositoryAdvisory> Advisories => _advisories;

        private readonly ILogger<GithubCacheService> _logger;

        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        private List<GitHub.Models.Release> _stableReleases = new();
        private List<GitHub.Models.Release> _nighltyReleases = new();
        private List<GitHub.Models.RepositoryAdvisory> _advisories = new();

        private static (string Owner, string Repo) MainRepo = ("vrcx-team", "VRCX");
        private static (string Owner, string Repo) OldRepo = ("Natsumi-sama", "VRCX");

        public GithubCacheService(ILogger<GithubCacheService> logger)
        {
            _logger = logger;
            _httpClient = new();

            // add authorization header
            _httpClient.DefaultRequestHeaders.Authorization = new("Bearer", CommonConfig.Config.Instance.GithubAPIKey);
            _httpClient.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github.v3+json");
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("VRCX-API");

            _jsonSerializerOptions = new(JsonSerializerDefaults.Web);
            _jsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
            _jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower));
            _jsonSerializerOptions.Converters.Add(new IgnoreEmptyStringNullableEnumConverter());
        }

        public async Task RefreshAsync()
        {
            await RefreshReleases();
            await RefreshAdvisories();

            GC.Collect(2, GCCollectionMode.Aggressive);
            GC.WaitForFullGCComplete();
        }

        private async Task RefreshReleases()
        {
            List<GitHub.Models.Release> stableReleases = [];
            List<GitHub.Models.Release> nighltyReleases = [];

            {
                var allReleases = await GetAllAsync<GitHub.Models.Release>($"https://api.github.com/repos/{MainRepo.Owner}/{MainRepo.Repo}/releases?per_page=100");
                stableReleases.AddRange(allReleases.Where(x => x.Prerelease == false));
                nighltyReleases.AddRange(allReleases.Where(x => x.Prerelease != false));
                allReleases.Clear();
            }
            {
                var allReleases = await GetAllAsync<GitHub.Models.Release>($"https://api.github.com/repos/{OldRepo.Owner}/{OldRepo.Repo}/releases?per_page=100");
                nighltyReleases.AddRange(allReleases);
                allReleases.Clear();
            }

            stableReleases = [.. stableReleases.Where(x => x.Draft != true).OrderByDescending(x => x.PublishedAt)];
            for (int i = 0; i < stableReleases.Count; i++)
            {
                stableReleases[i].Prerelease = false;
            }

            //_stableReleases.Clear();
            //_stableReleases.AddRange(stableReleases);
            //stableReleases.Clear();
            _stableReleases = stableReleases;
            _logger.LogInformation("Stabe Releases: {count}; Latest: {latestName}", _stableReleases.Count, _stableReleases.FirstOrDefault()?.Name);

            nighltyReleases = [.. nighltyReleases.Where(x => x.Draft != true).OrderByDescending(x => x.PublishedAt)];
            for (int i = 0; i < nighltyReleases.Count; i++)
            {
                nighltyReleases[i].Prerelease = false;
            }

            //_nighltyReleases.Clear();
            //_nighltyReleases.AddRange(nighltyReleases);
            //nighltyReleases.Clear();
            _nighltyReleases = nighltyReleases;
            _logger.LogInformation("Nightly Releases: {count}; Latest: {latestName}", _nighltyReleases.Count, _nighltyReleases.FirstOrDefault()?.Name);
        }

        private async Task RefreshAdvisories()
        {
            var advisories = await GetAllAsync<GitHub.Models.RepositoryAdvisory>($"https://api.github.com/repos/{MainRepo.Owner}/{MainRepo.Repo}/security-advisories");
            //_advisories.Clear();
            //_advisories.AddRange(advisories);
            //advisories.Clear();
            _advisories = advisories;
            _logger.LogInformation("Advisories: {count}; Latest: {latestName}", _advisories.Count, _advisories.FirstOrDefault()?.CveId);
        }

        private async Task<List<T>> GetAllAsync<T>(string path)
        {
            var result = new List<T>();

            string? next = path;
            while (true)
            {
                _logger.LogInformation("GET REQUEST: {path}", next);
                using var request = new HttpRequestMessage(HttpMethod.Get, next);
                using var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var items = await response.Content.ReadFromJsonAsync<List<T>>(_jsonSerializerOptions);
                if (items == null || items.Count == 0)
                {
                    break;
                }

                result.AddRange(items);

                if (!response.Headers.TryGetValues("Link", out var values) || !values.Any())
                {
                    break;
                }

                var link = LinkHeader.FromHeader(values.Single());
                if (link == null || string.IsNullOrWhiteSpace(link.NextLink))
                {
                    break;
                }

                next = link.NextLink;
            }

            return result;
        }
    }
}
