using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using AutoMapper;
using Gcpe.Hub.Data.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Gcpe.Hub.API.Controllers
{
    [Route("api/[Controller]")]
    [ApiController]
    [Produces("application/json")]
    public class SocialMediaPostsController : BaseController
    {
        private readonly HubDbContext dbContext;
        private readonly ILogger<SocialMediaPostsController> logger;
        private readonly IMapper mapper;

        public SocialMediaPostsController(HubDbContext dbContext, ILogger<SocialMediaPostsController> logger, IMapper mapper)
        {
            this.dbContext = dbContext;
            this.logger = logger;
            this.mapper = mapper;
        }

        const int checkInterval = 5;
        [HttpGet]
        [Produces(typeof(IEnumerable<Models.SocialMediaPost>))]
        [ProducesResponseType(304)]
        [ProducesResponseType(400)]
        [ResponseCache(Duration = checkInterval)]
        public IActionResult GetAllSocialMediaPosts()
        {
            try
            {
                IQueryable<SocialMediaPost> dbPosts = dbContext.SocialMediaPost.Where(p => p.IsActive);

                IActionResult res = HandleModifiedSince(checkInterval, () => dbPosts.OrderByDescending(p => p.Timestamp).FirstOrDefault()?.Timestamp);
                return res ?? Ok(mapper.Map<List<Models.SocialMediaPost>>(dbPosts.OrderBy(p => p.SortOrder).ToList()));
            }
            catch (Exception ex)
            {
                return this.BadRequest(logger, "Failed to retrieve social media posts", ex);
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(Models.SocialMediaPost), 201)]
        [ProducesResponseType(400)]
        public IActionResult AddSocialMediaPost(Models.SocialMediaPost socialMediaPost)
        {
            try
            {
                if (socialMediaPost.Id != Guid.Empty)
                {
                    throw new ValidationException("Invalid parameter (id)");
                }
                var dbPost = mapper.Map<SocialMediaPost>(socialMediaPost);
                dbPost.Id = Guid.NewGuid();
                dbPost.IsActive = true;
                dbContext.SocialMediaPost.Add(dbPost);
                dbContext.SaveChanges();
                return CreatedAtRoute("GetSocialMediaPost", new { id = dbPost.Id }, mapper.Map<Models.SocialMediaPost>(dbPost));
            }
            catch (Exception ex)
            {
                return this.BadRequest(logger, "Failed to create social media post", ex);
            }
        }

        [HttpGet("{id}", Name = "GetSocialMediaPost")]
        [Produces(typeof(Models.SocialMediaPost))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult GetSocialMediaPost(Guid id)
        {
            try
            {
                var dbPost = dbContext.SocialMediaPost.Find(id);
                if (dbPost != null && dbPost.IsActive)
                {
                    return Ok(mapper.Map<Models.SocialMediaPost>(dbPost));
                }
                return NotFound($"Social media post not found with id: {id}");
            }
            catch (Exception ex)
            {
                return this.BadRequest(logger, "Failed to retrieve social media post", ex);
            }
        }

        [HttpPut("{id}")]
        [Produces(typeof(Models.SocialMediaPost))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult UpdateSocialMediaPost(Guid id, Models.SocialMediaPost socialMediaPost)
        {
            try
            {
                var dbPost = dbContext.SocialMediaPost.Find(id);
                if (dbPost != null && dbPost.IsActive)
                {
                    dbPost = mapper.Map(socialMediaPost, dbPost);
                    dbPost.Timestamp = DateTime.Now;
                    dbPost.Id = id;
                    dbContext.SocialMediaPost.Update(dbPost);
                    dbContext.SaveChanges();
                    return Ok(mapper.Map<Models.SocialMediaPost>(dbPost));
                }
                return NotFound($"Social media post not found with id: {id}");
            }
            catch (Exception ex)
            {
                return this.BadRequest(logger, "Failed to update social media post", ex);
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult DeleteSocialMediaPost(Guid id)
        {
            try
            {
                var dbPost = dbContext.SocialMediaPost.Find(id);
                if (dbPost != null && dbPost.IsActive)
                {
                    dbPost.IsActive = false;
                    dbPost.Timestamp = DateTime.Now;
                    dbContext.SocialMediaPost.Update(dbPost);
                    dbContext.SaveChanges();
                    return NoContent();
                }
                return NotFound($"Social media post not found with id: {id}");
            }
            catch (Exception ex)
            {
                return this.BadRequest(logger, "Failed to delete social media post", ex);
            }
        }
    }
}
