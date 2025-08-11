using System.Text.Json;
using System.Text.Json.Serialization;
using VRCX_API.Configs;
using VRCX_API.Helpers;

namespace VRCX_API.Services
{
    public class ReleaseAsset : GitHub.Models.ReleaseAsset
    {
        public string? Digest { get; set; }
    }
    
    public class Release : GitHub.Models.Release
    {
        public List<ReleaseAsset>? Assets { get; set; }
    }
    
    public class GithubCacheService
    {
        public IReadOnlyCollection<Release> StableReleases => _stableReleases;
        public IReadOnlyCollection<Release> NightlyReleases => _nighltyReleases;
        public IReadOnlyCollection<GitHub.Models.RepositoryAdvisory> Advisories => _advisories;

        private readonly ILogger<GithubCacheService> _logger;

        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        private List<Release> _stableReleases = new();
        private List<Release> _nighltyReleases = new();
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

        public async Task<bool> RefreshAsync()
        {
            bool hasChanged = false;

            hasChanged |= await RefreshReleases();
            hasChanged |= await RefreshAdvisories();

            return hasChanged;
        }

        public async Task<bool> RefreshReleases()
        {
            bool hasChanged = false;

            List<Release> stableReleases = [];
            List<Release> nighltyReleases = [];

            {
                var allReleases = await GetAllAsync<Release>($"https://api.github.com/repos/{MainRepo.Owner}/{MainRepo.Repo}/releases?per_page=100");
                stableReleases.AddRange(allReleases.Where(x => x.Prerelease == false));
                nighltyReleases.AddRange(allReleases.Where(x => x.Prerelease != false));
                allReleases.Clear();
            }
            {
                var allReleases = await GetAllAsync<Release>($"https://api.github.com/repos/{OldRepo.Owner}/{OldRepo.Repo}/releases?per_page=100");
                nighltyReleases.AddRange(allReleases);
                allReleases.Clear();
            }

            stableReleases = [.. stableReleases.Where(x => x.Draft != true).OrderByDescending(x => x.PublishedAt)];
            for (int i = 0; i < stableReleases.Count; i++)
            {
                stableReleases[i].Prerelease = false;
            }

            hasChanged |= !AreEqual(stableReleases.FirstOrDefault(), _stableReleases.FirstOrDefault());

            _stableReleases = stableReleases;
            _logger.LogInformation("Stabe Releases: {count}; Latest: {latestName}", _stableReleases.Count, _stableReleases.FirstOrDefault()?.Name);

            nighltyReleases = [.. nighltyReleases.Where(x => x.Draft != true).OrderByDescending(x => x.PublishedAt)];
            for (int i = 0; i < nighltyReleases.Count; i++)
            {
                nighltyReleases[i].Prerelease = false;
            }

            hasChanged |= !AreEqual(nighltyReleases.FirstOrDefault(), _nighltyReleases.FirstOrDefault());

            _nighltyReleases = nighltyReleases;
            _logger.LogInformation("Nightly Releases: {count}; Latest: {latestName}", _nighltyReleases.Count, _nighltyReleases.FirstOrDefault()?.Name);

            return hasChanged;
        }

        private async Task<bool> RefreshAdvisories()
        {
            bool hasChanged = false;

            var advisories = await GetAllAsync<GitHub.Models.RepositoryAdvisory>($"https://api.github.com/repos/{MainRepo.Owner}/{MainRepo.Repo}/security-advisories");

            hasChanged |= !AreEqual(advisories.FirstOrDefault(), _advisories.FirstOrDefault());

            _advisories = advisories;
            _logger.LogInformation("Advisories: {count}; Latest: {latestName}", _advisories.Count, _advisories.FirstOrDefault()?.CveId);

            return hasChanged;
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

        private static bool AreEqual(Release? a, Release? b)
        {
            if (a == null && b == null)
            {
                return false;
            }

            if (a == null || b == null)
            {
                return false;
            }

            if (a.Id != b.Id ||
                   a.Name != b.Name ||
                   a.Body != b.Body ||
                   a.PublishedAt != b.PublishedAt ||
                   a.TagName != b.TagName ||
                   a.Assets?.Count != b.Assets?.Count)
            {
                return false;
            }

            if (a.Assets?.Count > 0 && b.Assets?.Count > 0)
            {
                for (int i = 0; i < a.Assets.Count; i++)
                {
                    if (a.Assets[i].Id != b.Assets[i].Id ||
                       a.Assets[i].Name != b.Assets[i].Name ||
                       a.Assets[i].UpdatedAt != b.Assets[i].UpdatedAt ||
                       a.Assets[i].State != b.Assets[i].State ||
                       a.Assets[i].BrowserDownloadUrl != b.Assets[i].BrowserDownloadUrl)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static bool AreEqual(GitHub.Models.RepositoryAdvisory? a, GitHub.Models.RepositoryAdvisory? b)
        {
            if (a == null && b == null)
            {
                return false;
            }

            if (a == null || b == null)
            {
                return false;
            }

            if (a.CveId != b.CveId ||
                   a.PublishedAt != b.PublishedAt ||
                   a.Severity != b.Severity ||
                   a.Summary != b.Summary ||
                   a.Description != b.Description ||
                   a.UpdatedAt != b.UpdatedAt ||
                   a.State != b.State ||
                   a.Vulnerabilities?.Count != b.Vulnerabilities?.Count)
            {
                return false;
            }

            if (a.Vulnerabilities?.Count > 0 && b.Vulnerabilities?.Count > 0)
            {
                for (int i = 0; i < a.Vulnerabilities.Count; i++)
                {
                    if (a.Vulnerabilities[i].PatchedVersions != b.Vulnerabilities[i].PatchedVersions ||
                       a.Vulnerabilities[i].Package?.Name != b.Vulnerabilities[i].Package?.Name ||
                       a.Vulnerabilities[i].VulnerableVersionRange != b.Vulnerabilities[i].VulnerableVersionRange)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
