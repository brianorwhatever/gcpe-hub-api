using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using FluentAssertions;
using Gcpe.Hub.API.Controllers;
using Gcpe.Hub.API.Helpers;
using Gcpe.Hub.Data.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Gcpe.Hub.API.Tests.ControllerTests
{
    public class ActivitiesControllerTests : IDisposable
    {
        private Mock<ILogger<ActivitiesController>> logger;
        private HubDbContext context;
        private IMapper mapper;
        private DbContextOptions<HubDbContext> options;

        public ActivitiesControllerTests()
        {
            options = new DbContextOptionsBuilder<HubDbContext>()
                      .UseInMemoryDatabase(Guid.NewGuid().ToString())
                      .Options;
            context = new HubDbContext(this.options);

            logger = new Mock<ILogger<ActivitiesController>>();

            var mockMapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new MappingProfile());
            });
            mapper = mockMapper.CreateMapper();
        }
        public void Dispose()
        {
            this.context.Dispose();
        }

        private ActivitiesController Controller(HubDbContext c = null)
        {
            return new ActivitiesController(c??context, logger.Object, mapper, null);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(3)]
        public void GetNewsForecast_ShouldReturnSuccess(int activityCount)
        {
            for (var i = 0; i < activityCount; i++)
            {
                var dbActivity = TestData.CreateDbActivity(i.ToString(), "test details", i + 1);
                context.Activity.Add(dbActivity);
            }
            context.SaveChanges();

            var result = Controller().GetActivityForecast(7) as ObjectResult;

            result.Should().BeOfType<OkObjectResult>();
            result.Should().NotBeNull();
            var models = result.Value as ICollection<Models.Activity>;
            models.Should().NotBeNull();
            models.Count().Should().Be(activityCount);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(7)]
        public void GetNewsForecast_ShouldFilterOnNumDays(int numDays)
        {
            var count = 3;
            for (var i = 0; i < count; i++)
            {
                var testDbActivity = TestData.CreateDbActivity($"title-{i.ToString()}", "test details", i + 1);
                context.Activity.Add(testDbActivity);
            }
            context.SaveChanges();

            var result = Controller().GetActivityForecast(numDays) as ObjectResult;

            result.Should().BeOfType<OkObjectResult>();
            var models = result.Value as ICollection<Models.Activity>;
            models.Count().Should().Be(numDays == 1 ? 0 : count);
        }

        [Fact]
        public void GetNewsForecast_ShouldReturnBadRequest()
        {
            var mockContext = new Mock<HubDbContext>(options);
            mockContext.Setup(m => m.Activity).Throws(new Exception());

            var result = Controller(mockContext.Object).GetActivityForecast(1) as ObjectResult;

            result.StatusCode.Should().Be(400);
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public void Post_ShouldReturnSuccess()
        {
            var testDbActivity = TestData.CreateDbActivity("test title", "test details", 1);
            context.Category.AddRange(testDbActivity.ActivityCategories.Select(ac => ac.Category));
            context.SaveChanges();

            var result = Controller().AddActivity(mapper.Map<Models.Activity>(testDbActivity)) as ObjectResult;

            result.StatusCode.Should().Be(201);
            result.Should().BeOfType<CreatedAtRouteResult>();
            var model = result.Value as Models.Activity;
            model.Title.Should().Be("test title");
        }

        [Fact]
        public void Get_ShouldReturnSuccess()
        {
            var testDbActivity = TestData.CreateDbActivity("test title", "test details", 1);
            context.Activity.Add(testDbActivity);
            context.SaveChanges();

            var result = Controller().GetActivity(testDbActivity.Id) as ObjectResult;

            result.Should().BeOfType<OkObjectResult>();
            result.StatusCode.Should().Be(200);
            var model = result.Value as Models.Activity;
            model.Title.Should().Be("test title");
        }

        [Fact]
        public void Get_ShouldReturnFail()
        {
            var testDbActivity = TestData.CreateDbActivity("test title", "test details", 2);
            context.Activity.Add(testDbActivity);
            context.SaveChanges();

            var mockContext = new Mock<HubDbContext>(options);
            mockContext.Setup(m => m.Activity).Throws(new Exception());
            var result = Controller(mockContext.Object).GetActivity(testDbActivity.Id) as ObjectResult;

            result.Should().BeOfType<BadRequestObjectResult>();
            result.StatusCode.Should().Be(400);
        }

        [Fact]
        public void Get_ShouldReturnNotFound()
        {
            var result = Controller().GetActivity(-1) as ObjectResult;

            result.Should().BeOfType<NotFoundObjectResult>();
            result.StatusCode.Should().Be(404);
        }
        
        [Fact]
        public void Put_ShouldReturnSuccess()
        {
            var testDbActivity = TestData.CreateDbActivity("test title", "test details", 1);

            context.Activity.Add(testDbActivity);
            context.Category.Add(new Category { Name = "New Category" });
            context.SaveChanges();

            var testActivity = mapper.Map<Models.Activity>(testDbActivity);
            testActivity.Title = "New Title!";
            testActivity.Categories = new string[] { "New Category" };


            var result = Controller().UpdateActivity(testDbActivity.Id, testActivity) as ObjectResult;

            result.Should().BeOfType<OkObjectResult>();
            result.StatusCode.Should().Be(200);
            var model = result.Value as Models.Activity;
            model.Title.Should().Be("New Title!");
            model.Categories.First().Should().Be("New Category");

            var DbActivity = context.Activity.Find(testDbActivity.Id);
            DbActivity.Title.Should().Be("New Title!");
        }
        
        [Fact]
        public void Put_ShouldReturnBadRequest()
        {
            var testDbActivity = TestData.CreateDbActivity("test title", "test details", 2);
            context.Activity.Add(testDbActivity);
            context.SaveChanges();

            var result = Controller().UpdateActivity(testDbActivity.Id, activity: null) as ObjectResult ;

            result.Should().BeOfType<BadRequestObjectResult>();
            result.StatusCode.Should().Be(400);
        }

        [Fact]
        public void Put_ShouldReturnNotFound()
        {
            var testDbActivity = TestData.CreateDbActivity("test title", "test details", 2);

            var result = Controller().UpdateActivity(testDbActivity.Id, activity: null) as ObjectResult;

            result.Should().BeOfType<NotFoundObjectResult>();
            result.StatusCode.Should().Be(404);
        }
    }
}
