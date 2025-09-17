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
        private string SentryDsnBase64 => Convert.ToBase64String(Encoding.UTF8.GetBytes(CommonConfig.Config.Instance.SentryDsn));

        public ErrorReportingController(ILogger<ErrorReportingController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [Route("getDsn")]
        [Produces(MediaTypeNames.Text.Plain)]
        public string GetDsn()
        {
            var referer = Request.Headers.Referer.ToString();
            var userAgent = Request.Headers.UserAgent.ToString();
            if (referer != "https://vrcx.app" && !userAgent.Contains("VRCX"))
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return string.Empty;
            }
            return SentryDsnBase64;
        }
    }
}
