using System.ComponentModel.DataAnnotations;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using VRCX_API.Services;

namespace VRCX_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StatusController : ControllerBase
    {
        private readonly ILogger<StatusController> _logger;
        private readonly VrChatStatusCacheService _vrChatStatusCacheService;

        public StatusController(ILogger<StatusController> logger, VrChatStatusCacheService vrChatStatusCacheService)
        {
            _logger = logger;
            _vrChatStatusCacheService = vrChatStatusCacheService;
        }

        [Route("statuses/{*statusPath}")]
        [HttpGet]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
        public ActionResult GetStatus([Required] string statusPath)
        {
            if (_vrChatStatusCacheService.Statuses.TryGetValue(statusPath, out var value))
            {
                return Content(value, MediaTypeNames.Application.Json);
            }
            return NotFound();
        }

        [Route("statuses")]
        [HttpGet]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IEnumerable<string> GetAllStatuses()
        {
            return _vrChatStatusCacheService.Statuses.Keys;
        }

        [Route("graphs/{*graphPath}")]
        [HttpGet]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
        public ActionResult GetGraph([Required] string graphPath)
        {
            if (_vrChatStatusCacheService.Graphs.TryGetValue(graphPath, out var value))
            {
                return Content(value, MediaTypeNames.Application.Json);
            }

            return NotFound();
        }

        [Route("graphs")]
        [HttpGet]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IReadOnlyDictionary<string, string> GetAllGraphs()
        {
            return _vrChatStatusCacheService.Graphs;
        }
    }
}
