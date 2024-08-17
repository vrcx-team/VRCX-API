using System.ComponentModel.DataAnnotations;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
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
        public IEnumerable<GitHub.Models.Release> GetStableReleases([Range(1, 101)] int page = 1, [Range(1, 101)] int pageSize = 30)
        {
            return _githubCacheService.StableReleases.Skip((page - 1) * pageSize).Take(pageSize);
        }

        [Route("stable/latest")]
        [HttpGet]
        [Produces(MediaTypeNames.Application.Json)]
        public GitHub.Models.Release? GetLastestStableRelease()
        {
            return _githubCacheService.StableReleases.FirstOrDefault();
        }

        [Route("nightly")]
        [HttpGet]
        [Produces(MediaTypeNames.Application.Json)]
        public IEnumerable<GitHub.Models.Release> GetNightlyReleases([Range(1, 101)] int page = 1, [Range(1, 101)] int pageSize = 30)
        {
            return _githubCacheService.NightlyReleases.Skip((page - 1) * pageSize).Take(pageSize);
        }

        [Route("nightly/latest")]
        [HttpGet]
        [Produces(MediaTypeNames.Application.Json)]
        public GitHub.Models.Release? GetLatestNightlyRelease()
        {
            return _githubCacheService.NightlyReleases.FirstOrDefault();
        }
    }
}
