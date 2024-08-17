using System.ComponentModel.DataAnnotations;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using VRCX_API.Services;

namespace VRCX_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SecurityController : ControllerBase
    {
        private readonly ILogger<ReleasesController> _logger;
        private readonly GithubCacheService _githubCacheService;

        public SecurityController(ILogger<ReleasesController> logger, GithubCacheService githubCacheService)
        {
            _logger = logger;
            _githubCacheService = githubCacheService;
        }

        [HttpGet]
        [Route("advisories")]
        [Produces(MediaTypeNames.Application.Json)]
        public IEnumerable<GitHub.Models.RepositoryAdvisory> GetRelaventAdvisories([Range(1, 101)] int page = 1, [Range(1, 101)] int pageSize = 30)
        {
            return _githubCacheService.Advisories.Skip((page - 1) * pageSize).Take(pageSize);
        }
    }
}
