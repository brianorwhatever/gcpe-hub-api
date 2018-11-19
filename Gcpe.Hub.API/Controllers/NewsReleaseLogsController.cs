using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Gcpe.Hub.API.Data;
using Gcpe.Hub.API.ViewModels;
using Gcpe.Hub.Data.Entity;
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
        public IActionResult GetAll(string newsReleaseId)
        {
            var release = _repository.GetReleaseByKey(newsReleaseId);
            if (release != null)
            {
                return Ok(_mapper.Map<IEnumerable<NewsReleaseLog>, IEnumerable<NewsReleaseLogViewModel>>(release.NewsReleaseLog));
            }
            return NotFound();
        }

        [HttpGet("{id}")]
        public IActionResult Get(string newsReleaseId, int id)
        {
            var release = _repository.GetReleaseByKey(newsReleaseId);
            if (release != null)
            {
                var log = release.NewsReleaseLog.Where(i => i.Id == id).FirstOrDefault();
                if (log != null)
                {
                    return Ok(_mapper.Map<NewsReleaseLog, NewsReleaseLogViewModel>(log));
                }
            }
            return NotFound();
        }


    }
}
