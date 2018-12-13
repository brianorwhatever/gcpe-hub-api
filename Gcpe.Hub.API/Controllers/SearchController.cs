using AutoMapper;
using Gcpe.Hub.API.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using Gcpe.Hub.Data.Entity;

namespace Gcpe.Hub.API.Controllers
{
    [Route("api/[Controller]")]
    [ApiController]
    [Produces("application/json")]
    public class SearchController : ControllerBase
    {
        private readonly HubDbContext dbContext;
        private readonly ILogger<SearchController> logger;
        private readonly IMapper mapper;

        public SearchController(HubDbContext dbContext,
            ILogger<SearchController> logger,
            IMapper mapper)
        {
            this.dbContext = dbContext;
            this.logger = logger;
            this.mapper = mapper;
        }

        [HttpGet]
        public IActionResult Search([FromQuery] SearchParams searchParams)
        {
            Thread.Sleep(1000); // sleep so it takes a second to return the results to the client
            return Ok();// _repository.GetAllArticles(searchParams));
        }

        [HttpGet("suggestions")]
        public IActionResult Suggestions()
        {
            var suggestions = ActivitiesController.QueryAll(dbContext)
                .Take(10)
                .Select(a => a.Title);

            return Ok(suggestions);
        }

    }
}
