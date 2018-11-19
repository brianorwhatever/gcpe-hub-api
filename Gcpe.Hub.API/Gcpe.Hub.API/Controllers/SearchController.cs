using AutoMapper;
using Gcpe.Hub.API.Helpers;
using Gcpe.Hub.API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;

namespace Gcpe.Hub.API.Controllers
{
    [Route("api/[Controller]")]
    [ApiController]
    [Produces("application/json")]
    public class SearchController : ControllerBase
    {
        private readonly IRepository _repository;
        private readonly ILogger<SearchController> _logger;
        private readonly IMapper _mapper;

        public SearchController(
            IRepository repository,
            ILogger<SearchController> logger,
            IMapper mapper)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult Search([FromQuery] SearchParams searchParams)
        {
            Thread.Sleep(1000); // sleep so it takes a second to return the results to the client
            return Ok(_repository.GetAllArticles(searchParams));
        }

        [HttpGet("suggestions")]
        public IActionResult Suggestions()
        {
            var suggestions = _repository.GetAllArticles(null)
                .Take(10)
                .Select(a => a.Title);

            return Ok(suggestions);
        }

    }
}
