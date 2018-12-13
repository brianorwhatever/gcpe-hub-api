using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Gcpe.Hub.API.Helpers;
using Gcpe.Hub.Data.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Gcpe.Hub.API.Controllers
{
    [Route("/api/newsreleases/{newsreleaseid}/logs")]
    [ApiController]
    public class NewsReleaseLogsController : ControllerBase
    {
        private readonly HubDbContext dbContext;
        private readonly ILogger<NewsReleaseLogsController> logger;
        private readonly IMapper mapper;

        public NewsReleaseLogsController(HubDbContext dbContext,
          ILogger<NewsReleaseLogsController> logger,
          IMapper mapper)
        {
            this.dbContext = dbContext;
            this.logger = logger;
            this.mapper = mapper;
        }

        [HttpGet]
        [Produces(typeof(IEnumerable<Models.NewsReleaseLog>))]
        public IActionResult GetPostLogs(string postKey)
        {
            var dbPost = dbContext.NewsRelease.Include(p => p.NewsReleaseLog).FirstOrDefault(p => p.Key == postKey);

            if (dbPost != null)
            {
                return Ok(mapper.Map<IEnumerable<Models.NewsReleaseLog>>(dbPost.NewsReleaseLog));
            }
            return NotFound();
        }

        [HttpPost]
        public IActionResult AddPostLog([FromBody]Models.NewsReleaseLog logEntry)
        {
            try
            {
                var dbPostLog = new NewsReleaseLog { DateTime = DateTimeOffset.Now};
                dbContext.Entry(dbPostLog).CurrentValues.SetValues(logEntry);
                dbContext.NewsReleaseLog.Add(dbPostLog);
                dbContext.SaveChanges();
                return CreatedAtRoute("GetPostLogs", new { key = logEntry.ReleaseKey }, mapper.Map<Models.NewsReleaseLog>(dbPostLog));
            }
            catch (Exception ex)
            {
                return this.BadRequest(logger, "Failed to save a new release", ex);
            }
        }
    }
}
