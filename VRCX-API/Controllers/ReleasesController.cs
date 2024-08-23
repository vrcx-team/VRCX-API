using System.ComponentModel.DataAnnotations;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using VRCX_API.Models;
using VRCX_API.Services;

namespace VRCX_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ReleasesController : ControllerBase
    {
        private readonly ILogger<ReleasesController> _logger;
        private readonly GithubCacheService _githubCacheService;

        public ReleasesController(ILogger<ReleasesController> logger, GithubCacheService githubCacheService)
        {
            _logger = logger;
            _githubCacheService = githubCacheService;
        }

        [Route("stable")]
        [HttpGet]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IEnumerable<GitHub.Models.Release> GetStableReleases([Range(1, 101)] int page = 1, [Range(1, 101)] int pageSize = 30)
        {
            return _githubCacheService.StableReleases.Skip((page - 1) * pageSize).Take(pageSize);
        }

        [Route("stable/latest")]
        [HttpGet]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public GitHub.Models.Release? GetLastestStableRelease()
        {
            return _githubCacheService.StableReleases.FirstOrDefault();
        }

        [Route("stable/latest/github")]
        [HttpGet]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status302Found)]
        [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
        public ActionResult GetLastestStableReleaseGithub()
        {
            var url = _githubCacheService.StableReleases.FirstOrDefault()?.HtmlUrl;

            if (url == null)
                return NotFound();

            return Redirect(url);
        }

        [Route("stable/latest/download")]
        [HttpGet]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status302Found)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
        public ActionResult GetLastestStableReleaseDownload(DownloadType type = DownloadType.Setup)
        {
            var url = type switch
            {
                DownloadType.Checksum => _githubCacheService.StableReleases.FirstOrDefault()?.Assets?.FirstOrDefault(x => !string.IsNullOrEmpty(x.Name) && x.Name.Equals("sha256sums.txt", StringComparison.OrdinalIgnoreCase))?.BrowserDownloadUrl,
                DownloadType.Setup => _githubCacheService.StableReleases.FirstOrDefault()?.Assets?.FirstOrDefault(x => !string.IsNullOrEmpty(x.Name) && x.Name.StartsWith("vrcx", StringComparison.OrdinalIgnoreCase) && x.Name.EndsWith("setup.exe", StringComparison.OrdinalIgnoreCase))?.BrowserDownloadUrl,
                DownloadType.Zip => _githubCacheService.StableReleases.FirstOrDefault()?.Assets?.FirstOrDefault(x => !string.IsNullOrEmpty(x.Name) && x.Name.StartsWith("vrcx", StringComparison.OrdinalIgnoreCase) && x.Name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))?.BrowserDownloadUrl,
                _ => null,
            };

            if (url == null)
                return NotFound();

            return Redirect(url);
        }

        [Route("nightly")]
        [HttpGet]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IEnumerable<GitHub.Models.Release> GetNightlyReleases([Range(1, 101)] int page = 1, [Range(1, 101)] int pageSize = 30)
        {
            return _githubCacheService.NightlyReleases.Skip((page - 1) * pageSize).Take(pageSize);
        }

        [Route("nightly/latest")]
        [HttpGet]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public GitHub.Models.Release? GetLatestNightlyRelease()
        {
            return _githubCacheService.NightlyReleases.FirstOrDefault();
        }

        [Route("nightly/latest/github")]
        [HttpGet]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status302Found)]
        [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
        public ActionResult GetLastestNightlyReleaseGithub()
        {
            var url = _githubCacheService.NightlyReleases.FirstOrDefault()?.HtmlUrl;

            if (url == null)
                return NotFound();

            return Redirect(url);
        }

        [Route("nightly/latest/download")]
        [HttpGet]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status302Found)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
        public ActionResult GetLastestNightlyReleaseDownload(DownloadType type = DownloadType.Setup)
        {
            var url = type switch
            {
                DownloadType.Checksum => _githubCacheService.NightlyReleases.FirstOrDefault()?.Assets?.FirstOrDefault(x => !string.IsNullOrEmpty(x.Name) && x.Name.Equals("sha256sums.txt", StringComparison.OrdinalIgnoreCase))?.BrowserDownloadUrl,
                DownloadType.Setup => _githubCacheService.NightlyReleases.FirstOrDefault()?.Assets?.FirstOrDefault(x => !string.IsNullOrEmpty(x.Name) && x.Name.StartsWith("vrcx", StringComparison.OrdinalIgnoreCase) && x.Name.EndsWith("setup.exe", StringComparison.OrdinalIgnoreCase))?.BrowserDownloadUrl,
                DownloadType.Zip => _githubCacheService.NightlyReleases.FirstOrDefault()?.Assets?.FirstOrDefault(x => !string.IsNullOrEmpty(x.Name) && x.Name.StartsWith("vrcx", StringComparison.OrdinalIgnoreCase) && x.Name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))?.BrowserDownloadUrl,
                _ => null,
            };

            if (url == null)
                return NotFound();

            return Redirect(url);
        }
    }
}
