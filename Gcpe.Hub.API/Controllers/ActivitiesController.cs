using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Gcpe.Hub.Data.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Gcpe.Hub.API.Helpers;

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

        internal static IQueryable<Activity> QueryAll(HubDbContext dbContext)
        {
            return dbContext.Activity.Include(a => a.ContactMinistry).Include(a => a.City)
                .Include(a => a.ActivityCategories).ThenInclude(ac => ac.Category)
                .Include(a => a.ActivitySharedWith).ThenInclude(sw => sw.Ministry);
        }


        [HttpGet("Forecast/{numDays}")]
        [Produces(typeof(IEnumerable<Models.Activity>))]
        [ProducesResponseType(400)]
        public IActionResult GetActivityForecast(int numDays)
        {
            try
            {
                var today = DateTime.Today.AddDays(-numDays/2); // temporary for testing with a stale db
                IList<Models.Activity> forecast = QueryAll(dbContext)
                    .Where(a => a.StartDateTime >= today && a.StartDateTime <= today.AddDays(numDays) && !a.IsConfidential && a.IsConfirmed && a.IsActive &&
//                               a.ActivityKeywords.Any(ak => ak.Keyword.Name.StartsWith("HQ-")) &&
                               a.ActivityCategories.Any(ac => ac.Category.Name.StartsWith("Approved") || ac.Category.Name == "Release Only (No Event)" || ac.Category.Name.EndsWith("with Release")))
                    .Select(a => mapper.Map<Models.Activity>(a)).ToList();

                return Ok(forecast);
            }
            catch (Exception ex)
            {
                return this.BadRequest(logger, "Failed to get activities", ex);
            }
        }

        [HttpGet("{id}")]
        [Produces(typeof(Models.Activity))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult GetActivity(int id)
        {
            try
            {
                var dbActivity = QueryAll(dbContext).FirstOrDefault(a => a.Id == id);

                if (dbActivity != null)
                {
                    return Ok(mapper.Map<Models.Activity>(dbActivity));
                }
                else return NotFound($"Activity not found with id: {id}");
            }
            catch (Exception ex)
            {
                return this.BadRequest(logger, "Failed to get an activity", ex);
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(Models.Activity), 201)]
        [ProducesResponseType(400)]
        public IActionResult AddActivity([FromBody]Models.Activity activity)
        {
            try
            {
                Activity dbActivity = new Activity { CreatedDateTime = DateTime.Now };
                dbActivity.UpdateFromModel(activity, dbContext);
                dbContext.Activity.Add(dbActivity);
                dbContext.SaveChanges();
                return CreatedAtRoute("GetActivity", new { id = activity.Id }, mapper.Map<Models.Activity>(dbActivity));
            }
            catch (Exception ex)
            {
                return this.BadRequest(logger, "Failed to save an activity", ex);
            }
        }

        [HttpPut("{id}")]
        [Produces(typeof(Models.Activity))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult UpdateActivity(int id, [FromBody] Models.Activity activity)
        {
            try
            {
                var dbActivity = dbContext.Activity.Find(id);
                if (dbActivity == null)
                {
                    return NotFound($"Could not find an activity with an id of {id}");
                }
                dbActivity.UpdateFromModel(activity, dbContext);
                dbContext.Activity.Update(dbActivity);
                dbContext.SaveChanges();
                return Ok(mapper.Map<Models.Activity>(dbActivity));
            }
            catch (Exception ex)
            {
                return this.BadRequest(logger, "Couldn't update activity", ex);
            }
        }
    }
}

