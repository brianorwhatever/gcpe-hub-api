using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Gcpe.Hub.API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Gcpe.Hub.API.Controllers
{
    [Route("/api/newsreleases/{newsreleaseid}/logs")]
    [ApiController]
    public class NewsReleaseLogsController : ControllerBase
    {
        private readonly IRepository _repository;
        private readonly ILogger<NewsReleaseLogsController> _logger;
        private readonly IMapper _mapper;

        public NewsReleaseLogsController(IRepository repository,
          ILogger<NewsReleaseLogsController> logger,
          IMapper mapper)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpGet]
        [Produces(typeof(IEnumerable<Models.NewsReleaseLog>))]
        public IActionResult GetAll(string newsReleaseId)
        {
            var dbRelease = _repository.GetReleaseByKey(newsReleaseId);
            if (dbRelease != null)
            {
                return Ok(_mapper.Map<IEnumerable<Models.NewsReleaseLog>>(dbRelease.NewsReleaseLog));
            }
            return NotFound();
        }

        [HttpGet("{id}")]
        [Produces(typeof(Models.NewsReleaseLog))]
        public IActionResult Get(string newsReleaseId, int id)
        {
            var dbRelease = _repository.GetReleaseByKey(newsReleaseId);
            if (dbRelease != null)
            {
                var dbLog = dbRelease.NewsReleaseLog.Where(i => i.Id == id).FirstOrDefault();
                if (dbLog != null)
                {
                    return Ok(_mapper.Map<Models.NewsReleaseLog>(dbLog));
                }
            }
            return NotFound();
        }


    }
}
