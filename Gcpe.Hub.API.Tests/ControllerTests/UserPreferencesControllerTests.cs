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
        private HttpContext httpContext;
        private ControllerContext controllerContext;

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

            httpContext = new DefaultHttpContext();
            controllerContext = new ControllerContext()
            {
                HttpContext = httpContext,
            };
            controller = new UserPreferencesController(context, logger.Object, mapper);
            controller.ControllerContext = controllerContext;
        }

        [Fact]
        public void Get_ShouldReturnSuccess()
        {
            var email = "test@gov.bc.ca";
            var testDbUserMinistryPreference = TestData.CreateUserMinistryPreference(email);
            context.UserMinistryPreference.Add(testDbUserMinistryPreference);
            context.SaveChanges();

            var token = generateToken(TokenType.Valid, email, "Test User");
            controller.Request.Headers["Authorization"] = $"Bearer {token}";

            var result = controller.GetUserMinistryPreferences() as ObjectResult;
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

            var token = generateToken(TokenType.Valid, "no_prefs@gov.bc.ca", "No Prefs User");
            controller.Request.Headers["Authorization"] = $"Bearer {token}";

            var result = controller.GetUserMinistryPreferences() as ObjectResult;
            result.Should().BeOfType<NotFoundObjectResult>();
            result.StatusCode.Should().Be(404);
        }

        [Fact]
        public void Get_ShouldReturnBadRequest()
        {
            var testDbUserMinistryPreference = TestData.CreateUserMinistryPreference();
            context.UserMinistryPreference.Add(testDbUserMinistryPreference);
            context.SaveChanges();

            var token = generateToken(TokenType.Invalid);
            controller.Request.Headers["Authorization"] = $"Bearer {token}";

            var result = controller.GetUserMinistryPreferences() as ObjectResult;
            result.Should().BeOfType<BadRequestObjectResult>();
            result.StatusCode.Should().Be(400);
        }

        [Fact]
        public void Post_ShouldReturnSuccess()
        {
            var testDbMinistry = TestData.CreateDbMinistries("Fake Ministry", Guid.NewGuid(), "fake-ministry");
            context.Ministry.Add(testDbMinistry);
            context.SaveChanges();

            var token = generateToken(TokenType.Valid, "test@gov.bc.ca", "Test User");
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

            var token = generateToken(TokenType.Invalid);
            controller.Request.Headers["Authorization"] = $"Bearer {token}";

            var result = controller.AddUserMinistryPreference(new string[] { "fake-ministry" }) as ObjectResult;
            result.Should().BeOfType<BadRequestObjectResult>();
            result.StatusCode.Should().Be(400);
        }


        private string generateToken(TokenType tokenType, string email = null, string username = null)
        {
            string token = "";
            const string secret = "GQDstcKsx0NHjPOuXOYg5MbeJ1XT0uFiwDVvVBrk";
            IJwtAlgorithm algorithm = new HMACSHA256Algorithm();
            IJsonSerializer serializer = new JsonNetSerializer();
            IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
            IJwtEncoder encoder = new JwtEncoder(algorithm, serializer, urlEncoder);

            switch (tokenType)
            {
                case TokenType.Valid:
                    token = encoder.Encode(new Dictionary<string, object>
                    {
                        { "preferred_username", email},
                        { "sub", "1234567890" },
                        { "name", username },
                        { "jti", Guid.NewGuid() },
                        { "iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
                        { "exp", DateTimeOffset.UtcNow.AddMinutes(30).ToUnixTimeSeconds() }
                    }, secret);
                    break;
                case TokenType.Invalid:
                    token = encoder.Encode(new Dictionary<string, object>
                    {
                        { "sub", "1234567890" },
                        { "jti", Guid.NewGuid() },
                        { "iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
                        { "exp", DateTimeOffset.UtcNow.AddMinutes(30).ToUnixTimeSeconds() }
                     }, secret);
                    break;
                default:
                    break;
            }
            return token;
        }

        private enum TokenType
        {
            Valid,
            Invalid
        }
    }
}
