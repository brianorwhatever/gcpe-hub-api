using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using AutoMapper;
using Gcpe.Hub.Data.Entity;
using Microsoft.AspNetCore.Authorization;
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
        private readonly IMapper mapper;
        static DateTime? lastModified = null;
        static DateTime lastModifiedNextCheck = DateTime.Now;

        public SocialMediaPostsController(HubDbContext dbContext, ILogger<SocialMediaPostsController> logger, IMapper mapper) : base(logger)
        {
            this.dbContext = dbContext;
            this.mapper = mapper;
        }

        [HttpGet]
        [Authorize(Policy = "ReadAccess")]
        [Produces(typeof(IEnumerable<Models.SocialMediaPost>))]
        [ProducesResponseType(304)]
        [ProducesResponseType(400)]
        [ResponseCache(Duration = 5)]
        public IActionResult GetAllSocialMediaPosts()
        {
            try
            {
                IQueryable<SocialMediaPost> dbPosts = dbContext.SocialMediaPost;

                IActionResult res = HandleModifiedSince(ref lastModified, ref lastModifiedNextCheck, () => dbPosts.OrderByDescending(p => p.Timestamp).FirstOrDefault()?.Timestamp);
                return res ?? Ok(mapper.Map<List<Models.SocialMediaPost>>(dbPosts.Where(p => p.IsActive).OrderBy(p => p.SortOrder).ThenByDescending(p => p.Timestamp).ToList()));
            }
            catch (Exception ex)
            {
                return BadRequest("Failed to retrieve social media posts", ex);
            }
        }

        [HttpPost]
        [Authorize(Policy = "WriteAccess")]
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
                var dbPost = new SocialMediaPost { IsActive = true };
                socialMediaPost.Id = Guid.NewGuid();
                socialMediaPost.Timestamp = DateTime.Now;
                dbContext.Entry(dbPost).CurrentValues.SetValues(socialMediaPost);
                dbContext.SocialMediaPost.Add(dbPost);
                dbContext.SaveChanges();
                return CreatedAtRoute("GetSocialMediaPost", new { id = dbPost.Id }, mapper.Map<Models.SocialMediaPost>(dbPost));
            }
            catch (Exception ex)
            {
                return BadRequest("Failed to create social media post", ex);
            }
        }

        [HttpGet("{id}", Name = "GetSocialMediaPost")]
        [Authorize(Policy = "ReadAccess")]
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
                return BadRequest("Failed to retrieve social media post", ex);
            }
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "WriteAccess")]
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
                    socialMediaPost.Id = id;
                    socialMediaPost.Timestamp = DateTime.Now;
                    dbContext.Entry(dbPost).CurrentValues.SetValues(socialMediaPost);
                    dbContext.SocialMediaPost.Update(dbPost);
                    dbContext.SaveChanges();
                    return Ok(mapper.Map<Models.SocialMediaPost>(dbPost));
                }
                return NotFound($"Social media post not found with id: {id}");
            }
            catch (Exception ex)
            {
                return BadRequest("Failed to update social media post", ex);
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "WriteAccess")]
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
                return BadRequest("Failed to delete social media post", ex);
            }
        }
    }
}
