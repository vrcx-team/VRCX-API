using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Mime;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using VRCX_API.Configs;
using VRCX_API.Services;

namespace VRCX_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ErrorReportingController : ControllerBase
    {
        private readonly ILogger<ErrorReportingController> _logger;
        private string SentryDsnBase64 => Convert.ToBase64String(Encoding.UTF8.GetBytes(VrcxConfig.Config.Instance.SentryDsn));

        public ErrorReportingController(ILogger<ErrorReportingController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [Route("getDsn")]
        [Produces(MediaTypeNames.Text.Plain)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
        public string? GetDsn()
        {
            var headers = Request.Headers;
            if(!headers.Referer.Any(x => x == "https://vrcx.app") && !headers.UserAgent.Any(x => x?.Contains("VRCX") == true))
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }

            return SentryDsnBase64;
        }
    }
}
