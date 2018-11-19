using Microsoft.AspNetCore.Mvc;

namespace Gcpe.Hub.API.Controllers
{
    [Route("")]
    [ApiController]
    [Produces("application/json")]
    public class HomeController : ControllerBase
    {
        [HttpGet]
        public IActionResult Index()
        {
            return new RedirectResult("~/swagger");
        }
    }
}

