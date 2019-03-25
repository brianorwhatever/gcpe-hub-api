using System;
using System.Collections.Generic;
using AutoMapper;
using FluentAssertions;
using Gcpe.Hub.API.Controllers;
using Gcpe.Hub.API.Helpers;
using Gcpe.Hub.Data.Entity;
using JWT;
using JWT.Algorithms;
using JWT.Serializers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Gcpe.Hub.API.Tests.ControllerTests
{
    public class UserPreferencesControllerTests
    {
        private Mock<ILogger<UserPreferencesController>> logger;
        private HubDbContext context;
        private UserPreferencesController controller;
        private IMapper mapper;
        private DbContextOptions<HubDbContext> options;

        public UserPreferencesControllerTests()
        {
            options = new DbContextOptionsBuilder<HubDbContext>()
                      .UseInMemoryDatabase(Guid.NewGuid().ToString())
                      .Options;
            context = new HubDbContext(options);
            logger = new Mock<ILogger<UserPreferencesController>>();
            var mockMapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new MappingProfile());
            });
            mapper = mockMapper.CreateMapper();
            controller = new UserPreferencesController(context, logger.Object, mapper);
        }

        [Fact]
        public void Get_ShouldReturnSuccess()
        {
            var testDbUserMinistryPreference = TestData.CreateUserMinistryPreference();
            context.UserMinistryPreference.Add(testDbUserMinistryPreference);
            context.SaveChanges();

            var httpContext = new DefaultHttpContext();
            var controllerContext = new ControllerContext()
            {
                HttpContext = httpContext,
            };
            controller.ControllerContext = controllerContext;

            var payload = new Dictionary<string, object>
            {
                { "preferred_username", "test@gov.bc.ca" },
                { "sub", "1234567890" },
                { "name", "Test User" },
                { "jti", Guid.NewGuid() },
                { "iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
                { "exp", DateTimeOffset.UtcNow.AddMinutes(30).ToUnixTimeSeconds() }
            };
            const string secret = "GQDstcKsx0NHjPOuXOYg5MbeJ1XT0uFiwDVvVBrk";

            IJwtAlgorithm algorithm = new HMACSHA256Algorithm();
            IJsonSerializer serializer = new JsonNetSerializer();
            IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
            IJwtEncoder encoder = new JwtEncoder(algorithm, serializer, urlEncoder);

            var token = encoder.Encode(payload, secret);

            controller.Request.Headers["Authorization"] = $"Bearer {token}";

            var result = controller.GetUserMinistryPreferences(false) as ObjectResult;

            result.Should().BeOfType<OkObjectResult>();
            result.StatusCode.Should().Be(200);
            result.Value.Should().BeOfType<List<string>>();
        }

        [Fact]
        public void Get_ShouldReturnNotFound()
        {
            var testDbUserMinistryPreference = TestData.CreateUserMinistryPreference();
            context.UserMinistryPreference.Add(testDbUserMinistryPreference);
            context.SaveChanges();

            var httpContext = new DefaultHttpContext();
            var controllerContext = new ControllerContext()
            {
                HttpContext = httpContext,
            };
            controller.ControllerContext = controllerContext;

            var payload = new Dictionary<string, object>
            {
                { "preferred_username", "no_prefs@gov.bc.ca" },
                { "sub", "1234567890" },
                { "name", "No Prefs User" },
                { "jti", Guid.NewGuid() },
                { "iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
                { "exp", DateTimeOffset.UtcNow.AddMinutes(30).ToUnixTimeSeconds() }
            };
            const string secret = "GQDstcKsx0NHjPOuXOYg5MbeJ1XT0uFiwDVvVBrk";

            IJwtAlgorithm algorithm = new HMACSHA256Algorithm();
            IJsonSerializer serializer = new JsonNetSerializer();
            IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
            IJwtEncoder encoder = new JwtEncoder(algorithm, serializer, urlEncoder);

            var token = encoder.Encode(payload, secret);

            controller.Request.Headers["Authorization"] = $"Bearer {token}";

            var result = controller.GetUserMinistryPreferences(false) as ObjectResult;

            result.Should().BeOfType<NotFoundObjectResult>();
            result.StatusCode.Should().Be(404);
        }

        [Fact]
        public void Get_ShouldReturnBadRequest()
        {
            var testDbUserMinistryPreference = TestData.CreateUserMinistryPreference();
            context.UserMinistryPreference.Add(testDbUserMinistryPreference);
            context.SaveChanges();

            var httpContext = new DefaultHttpContext();
            var controllerContext = new ControllerContext()
            {
                HttpContext = httpContext,
            };
            controller.ControllerContext = controllerContext;

            var payload = new Dictionary<string, object>
            {
                { "sub", "1234567890" },
                { "jti", Guid.NewGuid() },
                { "iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
                { "exp", DateTimeOffset.UtcNow.AddMinutes(30).ToUnixTimeSeconds() }
            };
            const string secret = "GQDstcKsx0NHjPOuXOYg5MbeJ1XT0uFiwDVvVBrk";

            IJwtAlgorithm algorithm = new HMACSHA256Algorithm();
            IJsonSerializer serializer = new JsonNetSerializer();
            IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
            IJwtEncoder encoder = new JwtEncoder(algorithm, serializer, urlEncoder);

            var invalidToken = encoder.Encode(payload, secret);

            controller.Request.Headers["Authorization"] = $"Bearer {invalidToken}";

            var result = controller.GetUserMinistryPreferences(false) as ObjectResult;

            result.Should().BeOfType<BadRequestObjectResult>();
            result.StatusCode.Should().Be(400);
        }


        [Fact]
        public void Post_ShouldReturnSuccess()
        {
            var testDbMinistry = TestData.CreateDbMinistries("Fake Ministry", Guid.NewGuid(), "fake-ministry");
            context.Ministry.Add(testDbMinistry);
            context.SaveChanges();

            var httpContext = new DefaultHttpContext();
            var controllerContext = new ControllerContext()
            {
                HttpContext = httpContext,
            };
            controller.ControllerContext = controllerContext;

            var payload = new Dictionary<string, object>
            {
                { "preferred_username", "test@gov.bc.ca" },
                { "sub", "1234567890" },
                { "name", "Test User" },
                { "jti", Guid.NewGuid() },
                { "iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
                { "exp", DateTimeOffset.UtcNow.AddMinutes(30).ToUnixTimeSeconds() }
            };
            const string secret = "GQDstcKsx0NHjPOuXOYg5MbeJ1XT0uFiwDVvVBrk";

            IJwtAlgorithm algorithm = new HMACSHA256Algorithm();
            IJsonSerializer serializer = new JsonNetSerializer();
            IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
            IJwtEncoder encoder = new JwtEncoder(algorithm, serializer, urlEncoder);

            var token = encoder.Encode(payload, secret);

            controller.Request.Headers["Authorization"] = $"Bearer {token}";

            var result = controller.AddUserMinistryPreference(new string[] { "fake-ministry" }) as ObjectResult;

            result.Should().BeOfType<CreatedAtRouteResult>();
            result.StatusCode.Should().Be(201);
        }

        [Fact]
        public void Post_ShouldReturnBadRequest()
        {
            var testDbMinistry = TestData.CreateDbMinistries("Fake Ministry", Guid.NewGuid(), "fake-ministry");
            context.Ministry.Add(testDbMinistry);
            context.SaveChanges();

            var httpContext = new DefaultHttpContext();
            var controllerContext = new ControllerContext()
            {
                HttpContext = httpContext,
            };
            controller.ControllerContext = controllerContext;

            var payload = new Dictionary<string, object>
            {
                { "sub", "1234567890" },
                { "jti", Guid.NewGuid() },
                { "iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
                { "exp", DateTimeOffset.UtcNow.AddMinutes(30).ToUnixTimeSeconds() }
            };
            const string secret = "GQDstcKsx0NHjPOuXOYg5MbeJ1XT0uFiwDVvVBrk";

            IJwtAlgorithm algorithm = new HMACSHA256Algorithm();
            IJsonSerializer serializer = new JsonNetSerializer();
            IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
            IJwtEncoder encoder = new JwtEncoder(algorithm, serializer, urlEncoder);

            var token = encoder.Encode(payload, secret);

            controller.Request.Headers["Authorization"] = $"Bearer {token}";

            var result = controller.AddUserMinistryPreference(new string[] { "fake-ministry" }) as ObjectResult;

            result.Should().BeOfType<BadRequestObjectResult>();
            result.StatusCode.Should().Be(400);
        }
    }
}
