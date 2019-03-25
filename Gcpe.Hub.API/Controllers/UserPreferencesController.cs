using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using AutoMapper;
using Gcpe.Hub.Data.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Gcpe.Hub.API.Controllers
{
    [Route("api/[Controller]")]
    [ApiController]
    [Produces("application/json")]
    public class UserPreferencesController : BaseController
    {
        private readonly HubDbContext dbContext;
        private readonly IMapper mapper;

        public UserPreferencesController(HubDbContext dbContext, ILogger<UserPreferencesController> logger, IMapper mapper) : base(logger)
        {
            this.dbContext = dbContext;
            this.mapper = mapper;
        }

        [HttpGet(Name = "GetUserMinistryPreferences")]
        [Authorize(Policy = "ReadAccess")]
        [Produces(typeof(string[]))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ResponseCache(Duration = 5)]
        public IActionResult GetUserMinistryPreferences(bool getAbbreviations = false)
        {
            try
            {
                var email = GetEmailAddressFromJWT(Request.Headers["Authorization"].FirstOrDefault().Split(' ')[1]);
                var dbUserMinistryPrefs = dbContext.UserMinistryPreference.Include(m => m.Ministry).Where(p => p.Email == email).ToList();
                if (dbUserMinistryPrefs.Any())
                {
                    var prefs = dbUserMinistryPrefs.Select(p => p.Ministry.DisplayName).ToList();
                    if (getAbbreviations == true)
                    {
                        prefs = dbUserMinistryPrefs.Select(p => p.Ministry.Abbreviation).ToList();
                    }
                    return Ok(prefs);
                }
                return NotFound($"Could not find preferences for user with email address: {email}");
            }
            catch (Exception ex)
            {
                return BadRequest("Failed to retrieve user ministry preferences", ex);
            }
        }

        [HttpPost]
        [Authorize(Policy = "WriteAccess")]
        [ProducesResponseType(typeof(string[]), 201)]
        [ProducesResponseType(400)]
        public IActionResult AddUserMinistryPreference(string[] ministryKeys)
        {
            try
            {
                var email = GetEmailAddressFromJWT(Request.Headers["Authorization"].FirstOrDefault().Split(' ')[1]);
                var dbUserMinistryPrefs = dbContext.UserMinistryPreference.Include(m => m.Ministry).Where(p => p.Email == email).ToList();
                dbContext.RemoveRange(dbUserMinistryPrefs);

                var userPrefs = new List<UserMinistryPreference>();
                foreach (var key in ministryKeys)
                {
                    var ministry = dbContext.Ministry.SingleOrDefault(m => m.Key == key);
                    userPrefs.Add(new UserMinistryPreference { Email = email, Ministry = ministry });
                }
                dbContext.AddRange(userPrefs);

                dbContext.SaveChanges();
                return CreatedAtRoute("GetUserMinistryPreferences", ministryKeys);
            }
            catch (Exception ex)
            {
                return BadRequest("Failed to create user ministry preferences", ex);
            }
        }

        private string GetEmailAddressFromJWT(string accessToken) => new JwtSecurityToken(accessToken)?.Claims.First(claim => claim.Type == "preferred_username").Value; // the preferred_username claim will differ depending on the auth provider
    }
}
