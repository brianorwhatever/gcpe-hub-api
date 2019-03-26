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
using Microsoft.Extensions.Primitives;

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
        [ResponseCache(Duration = 5)]
        public IActionResult GetUserMinistryPreferences()
        {
            try
            {
                var email = GetEmailAddressFromAuthorizationHeader(Request.Headers["Authorization"]);
                var dbUserMinistryPrefs = dbContext.UserMinistryPreference.Include(m => m.Ministry).Where(p => p.Email == email).ToList();
                if (dbUserMinistryPrefs.Any())
                    return Ok(dbUserMinistryPrefs.Select(p => p.Ministry.Key).ToList());

                return Ok(dbUserMinistryPrefs);
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
                var header = Request.Headers["Authorization"];
                var email = GetEmailAddressFromAuthorizationHeader(Request.Headers["Authorization"]);
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

        private string GetEmailAddressFromAuthorizationHeader(StringValues authorizationHeader)
            => new JwtSecurityToken(
                authorizationHeader.FirstOrDefault().Split(' ')[1])?.Claims
                    .First(claim => claim.Type == "preferred_username").Value; // the preferred_username claim will differ depending on the auth provider
    }
}
