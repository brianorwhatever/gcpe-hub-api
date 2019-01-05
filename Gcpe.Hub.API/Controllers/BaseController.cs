using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Gcpe.Hub.API.Controllers
{
    /// <summary>
    /// The base class for all controllers
    /// </summary>
    public abstract class BaseController : ControllerBase
    {
        static DateTime? lastModified = null;
        static DateTime lastModifiedNextCheck = DateTime.Now;

        /// <summary>
        /// HandleModifiedSince, caching support
        /// </summary>
        /// <param name="model"></param>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        protected IActionResult HandleModifiedSince(int checkInterval, Func<DateTime?> lastModifiedUpdateFn)
        {
            var now = DateTime.Now;
            if (lastModifiedNextCheck <= now)
            {
                lastModified = lastModifiedUpdateFn();
                lastModifiedNextCheck = now.AddSeconds(checkInterval);
            }
            if (lastModified.HasValue && Request != null)
            {
                var modifiedSpan = lastModified - Request.GetTypedHeaders().IfModifiedSince;

                // Ignore milliseconds because browsers are not supposed to store them (HTTP-date: https://www.w3.org/Protocols/rfc2616/rfc2616-sec3.html#sec3.3.1)
                if (modifiedSpan.HasValue && modifiedSpan.Value.TotalMilliseconds < 1000)
                {
                    return StatusCode(StatusCodes.Status304NotModified);
                }
                Response.GetTypedHeaders().LastModified = lastModified;
            }
            return null;
        }

        protected IActionResult BadRequest(ILogger logger, string error, Exception ex)
        {
            logger.LogError(error + ": " + ex.ToString());
            return BadRequest(error);
        }
    }
}
