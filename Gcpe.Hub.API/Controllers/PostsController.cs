using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using AutoMapper;
using Gcpe.Hub.API.Helpers;
using Gcpe.Hub.Data.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Gcpe.Hub.API.Controllers
{
    // TODO: Re-enable this ==> [Authorize]
    [Route("api/[Controller]")]
    [ApiController]
    [Produces("application/json")]
    public class PostsController : ControllerBase
    {
        private readonly HubDbContext dbContext;
        private readonly ILogger<PostsController> logger;
        private readonly IMapper mapper;

        public PostsController(HubDbContext dbContext,
            ILogger<PostsController> logger,
            IMapper mapper)
        {
            this.dbContext = dbContext;
            this.logger = logger;
            this.mapper = mapper;
        }

        private IQueryable<NewsRelease> QueryAll()
        {
            return dbContext.NewsRelease.Include(p => p.Ministry).Include(p => p.NewsReleaseLanguage).Include(p => p.NewsReleaseMinistry)
                .Include(p => p.NewsReleaseDocument).ThenInclude(nrd => nrd.NewsReleaseDocumentLanguage)
                .Include(p => p.NewsReleaseDocument).ThenInclude(nrd => nrd.NewsReleaseDocumentContact)
                .Where(p => p.IsCommitted && p.IsPublished);
        }
        [NonAction]
        public IList<Models.Post> GetResultsPage(NewsReleaseParams newsReleaseParams)
        {
            var posts = QueryAll();
            var pagedPosts = PagedList<NewsRelease>.Create(posts, newsReleaseParams.PageNumber, newsReleaseParams.PageSize);
            return pagedPosts.Select(p => p.ToModel(mapper)).ToList();
        }

        [HttpGet]
        [Produces(typeof(IEnumerable<Models.Post>))]
        [ProducesResponseType(400)]
        public IActionResult GetAll([FromQuery] NewsReleaseParams newsReleaseParams)
        {
            try
            {
                var count = QueryAll().Count();
                var pagedPosts = this.GetResultsPage(newsReleaseParams);
                Response.AddPagination(newsReleaseParams.PageNumber, newsReleaseParams.PageSize, count, 10);

                return Ok(pagedPosts);
            }
            catch (Exception ex)
            {
                return this.BadRequest(logger, "Failed to get all posts", ex);
            }
        }

        [HttpGet("Latest/{numDays}")]
        [Produces(typeof(IEnumerable<Models.Post>))]
        [ProducesResponseType(400)]
        public IActionResult GetLatestPosts(int numDays)
        {
            try
            {
                var today = DateTime.Today;

                IList<Models.Post> latest = QueryAll().Where(p => p.PublishDateTime >= today.AddDays(-numDays))
                    .Select(p => p.ToModel(mapper)).ToList();

                return Ok(latest);
            }
            catch (Exception ex)
            {
                return this.BadRequest(logger, "Failed to get latest posts", ex);
            }
        }

        [HttpGet("{key}", Name = "GetPost")]
        [Produces(typeof(Models.Post))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult GetPost(string key)
        {
            try
            {
                var dbPost = QueryAll().FirstOrDefault(p => p.Key == key);

                if (dbPost != null)
                {
                    var model = dbPost.ToModel(mapper);
                    return Ok(model);
                }
                else return NotFound($"Post not found with key: {key}");
            }
            catch (Exception ex)
            {
                return this.BadRequest(logger, "Failed to get post", ex);
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(Models.Post), 201)]
        [ProducesResponseType(400)]
        public IActionResult AddPost(Models.Post post)
        {
            try
            {
                if (post == null)
                {
                    throw new ValidationException();
                }
                var dbPost = new NewsRelease { Id = Guid.NewGuid(), IsPublished = true };
                dbPost.UpdateFromModel(post, dbContext);
                dbContext.NewsRelease.Add(dbPost);
                dbContext.SaveChanges();
                return CreatedAtRoute("GetPost", new { key = dbPost.Key }, dbPost.ToModel(mapper));
            }
            catch (Exception ex)
            {
                return this.BadRequest(logger, "Failed to save a new post", ex);
            }
        }

        [HttpPut("{key}")]
        [Produces(typeof(Models.Post))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult UpdatePost(string key, [FromBody] Models.Post post)
        {
            try
            {
                var dbPost = dbContext.NewsRelease.Include(p => p.NewsReleaseMinistry).FirstOrDefault(p => p.Key == post.Key);
                if (dbPost == null)
                {
                    return NotFound($"Could not find a post with a key of {key}");
                }

                dbPost.UpdateFromModel(post, dbContext);
                dbContext.NewsRelease.Update(dbPost);
                dbContext.SaveChanges();
                return Ok(dbPost.ToModel(mapper));
            }
            catch (Exception ex)
            {
                return this.BadRequest(logger, "Couldn't update post", ex);
            }
        }

    }
}

