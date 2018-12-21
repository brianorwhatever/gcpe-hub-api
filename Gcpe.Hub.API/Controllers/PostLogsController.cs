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
    [Route("/api/Posts/{newsreleaseid}/logs")]
    [ApiController]
    public class PostLogsController : ControllerBase
    {
        private readonly HubDbContext dbContext;
        private readonly ILogger<PostLogsController> logger;
        private readonly IMapper mapper;

        public PostLogsController(HubDbContext dbContext,
          ILogger<PostLogsController> logger,
          IMapper mapper)
        {
            this.dbContext = dbContext;
            this.logger = logger;
            this.mapper = mapper;
        }

        [HttpGet("{postKey}")]
        [Produces(typeof(IEnumerable<Models.PostLog>))]
        [ResponseCache(Duration = 300)] // change to 10 when using swagger
        public IActionResult GetPostLogs(string postKey)
        {
            var dbPost = dbContext.NewsRelease.Include(p => p.NewsReleaseLog).FirstOrDefault(p => p.Key == postKey);

            if (dbPost != null)
            {
                return Ok(mapper.Map<IEnumerable<Models.PostLog>>(dbPost.NewsReleaseLog));
            }
            return NotFound();
        }

        [HttpPost]
        public IActionResult AddPostLog([FromBody]Models.PostLog logEntry)
        {
            try
            {
                var dbPost = dbContext.NewsRelease.Include(p => p.NewsReleaseLog).FirstOrDefault(p => p.Key == logEntry.PostKey);
                var dbPostLog = new NewsReleaseLog { DateTime = DateTimeOffset.Now };
                dbContext.Entry(dbPostLog).CurrentValues.SetValues(logEntry);
                dbPost.NewsReleaseLog.Add(dbPostLog);
                dbContext.SaveChanges();
                return CreatedAtRoute("GetPostLogs", new { key = logEntry.PostKey }, mapper.Map<Models.PostLog>(dbPostLog));
            }
            catch (Exception ex)
            {
                return this.BadRequest(logger, "Failed to save a new post log entry", ex);
            }
        }
    }
}
