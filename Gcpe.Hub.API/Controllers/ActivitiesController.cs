using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Gcpe.Hub.API.ViewModels;
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
    public class ActivitiesController : ControllerBase
    {
        private readonly HubDbContext dbContext;
        private readonly ILogger<ActivitiesController> logger;
        private readonly IMapper mapper;

        public ActivitiesController(HubDbContext dbContext,
            ILogger<ActivitiesController> logger,
            IMapper mapper)
        {
            this.dbContext = dbContext;
            this.logger = logger;
            this.mapper = mapper;
        }

        [HttpGet("Forecast/{numDays}")]
        [Produces(typeof(IEnumerable<ActivityViewModel>))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public IActionResult GetNewsForecast(int numDays)
        {
            try
            {
                var today = new DateTime(2018, 9, 1);// DateTime.Today; // for testing
                IList<ActivityViewModel> forecast = dbContext.Activity.Include(a => a.ContactMinistry).Include(a => a.City)
                    .Include(a => a.ActivityCategories).ThenInclude(ac => ac.Category)
                    .Include(a => a.ActivitySharedWith).ThenInclude(sw => sw.Ministry)
                    .Where(a => a.StartDateTime >= today && a.StartDateTime <= today.AddDays(numDays) && !a.IsConfidential && a.IsConfirmed && a.IsActive &&
//                               a.ActivityKeywords.Any(ak => ak.Keyword.Name.StartsWith("HQ-")) &&
                               a.ActivityCategories.Any(ac => ac.Category.Name.StartsWith("Approved") || ac.Category.Name == "Release Only (No Event)" || ac.Category.Name.EndsWith("with Release")))
                    .Select(a => mapper.Map<Activity, ActivityViewModel>(a)).ToList();

                return Ok(forecast);
            }
            catch (Exception ex)
            {
                logger.LogError($"Failed to get activities: {ex}");
                return BadRequest("Failed to get activities");
            }
        }

        [HttpGet("{id}")]
        [Produces(typeof(ActivityViewModel))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult GetById(int id)
        {
            try
            {
                var activity = dbContext.Activity.Include(a => a.ContactMinistry).Include(a => a.City)
                    .Include(a => a.ActivityCategories).ThenInclude(ac => ac.Category)
                    .Include(a => a.ActivitySharedWith).ThenInclude(sw => sw.Ministry)
                    .FirstOrDefault(a => a.Id == id);

                if (activity != null)
                {
                    return Ok(mapper.Map<Activity, ActivityViewModel>(activity));
                }
                else return NotFound();
            }
            catch (Exception ex)
            {
                logger.LogError($"Failed to get activity: {ex}");
                return BadRequest("Failed to get activity");
            }
        }

        [HttpPost]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        public IActionResult Post([FromBody]ActivityViewModel activity)
        {
            try
            {
                dbContext.Activity.Add(mapper.Map<ActivityViewModel, Activity>(activity));
                dbContext.SaveChanges();
                return CreatedAtRoute("GetActivity", new { id = activity.Id }, activity);
            }
            catch (Exception ex)
            {
                logger.LogError($"Failed to save a activities: {ex}");
            }

            return BadRequest("Failed to save activities");
        }

        [HttpPut("{id}")]
        [Produces(typeof(ActivityViewModel))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult Put(int id, [FromBody] ActivityViewModel activity)
        {
            try
            {
                var oldActivity = dbContext.Activity.Find(id);
                if (oldActivity == null)
                {
                    return NotFound($"Could not find an activity with an id of {id}");
                }
                dbContext.Activity.Update(mapper.Map<ActivityViewModel, Activity>(activity));
                dbContext.SaveChanges();
                return Ok(activity);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
            }

            return BadRequest("Couldn't update activity");
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult Delete(int id)
        {
            try
            {
                var oldActivity = dbContext.Activity.Find(id);
                if (oldActivity == null)
                {
                    return NotFound($"Could not find activity with id of {id}");
                }
                dbContext.Activity.Remove(oldActivity);
                dbContext.SaveChanges();
                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
            }

            return BadRequest("Could not delete activity");
        }

    }
}

