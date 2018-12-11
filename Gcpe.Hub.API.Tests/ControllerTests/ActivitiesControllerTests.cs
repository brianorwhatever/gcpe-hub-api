using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using FluentAssertions;
using Gcpe.Hub.API.Controllers;
using Gcpe.Hub.API.Data;
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
            var controller = new ActivitiesController(context, logger.Object, mapper);

            var result = controller.GetActivityForecast(7) as ObjectResult;

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
            var controller = new ActivitiesController(context, logger.Object, mapper);

            var result = controller.GetActivityForecast(numDays) as ObjectResult;

            result.Should().BeOfType<OkObjectResult>();
            var models = result.Value as ICollection<Models.Activity>;
            models.Count().Should().Be(numDays == 1 ? 0 : count);
        }

        [Fact]
        public void GetNewsForecast_ShouldReturnBadRequest()
        {
            var mockContext = new Mock<HubDbContext>(options);
            mockContext.Setup(m => m.Activity).Throws(new Exception());
            var controller = new ActivitiesController(mockContext.Object, logger.Object, mapper);

            var result = controller.GetActivityForecast(1) as ObjectResult;

            result.StatusCode.Should().Be(400);
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public void Post_ShouldReturnSuccess()
        {
            var controller = new ActivitiesController(context, logger.Object, mapper);
            var testDbActivity = TestData.CreateDbActivity("test title", "test details", 1);

            var result = controller.AddActivity(mapper.Map<Models.Activity>(testDbActivity)) as ObjectResult;

            result.StatusCode.Should().Be(201);
            result.Should().BeOfType<CreatedAtRouteResult>();
            var model = result.Value as Models.Activity;
            model.Title.Should().Be("test title");
        }

        [Fact]
        public void Get_ShouldReturnSuccess()
        {
            var controller = new ActivitiesController(context, logger.Object, mapper);
            var testDbActivity = TestData.CreateDbActivity("test title", "test details", 1);
            context.Activity.Add(testDbActivity);
            context.SaveChanges();

            var result = controller.GetActivity(testDbActivity.Id) as ObjectResult;

            result.Should().BeOfType<OkObjectResult>();
            result.StatusCode.Should().Be(200);
            var model = result.Value as Models.Activity;
            model.Title.Should().Be("test title");
        }

        [Fact]
        public void Get_ShouldReturnFail()
        {
            var mockContext = new Mock<HubDbContext>(options);
            mockContext.Setup(m => m.Activity).Throws(new Exception());
            var controller = new ActivitiesController(mockContext.Object, logger.Object, mapper);
            var testDbActivity = TestData.CreateDbActivity("test title", "test details", 2);
            context.Activity.Add(testDbActivity);
            context.SaveChanges();

            var result = controller.GetActivity(testDbActivity.Id) as ObjectResult;

            result.Should().BeOfType<BadRequestObjectResult>();
            result.StatusCode.Should().Be(400);
        }

        [Fact]
        public void Get_ShouldReturnNotFound()
        {
            var controller = new ActivitiesController(context, logger.Object, mapper);

            var result = controller.GetActivity(-1) as ObjectResult;

            result.Should().BeOfType<NotFoundObjectResult>();
            result.StatusCode.Should().Be(404);
        }
        
        [Fact]
        public void Put_ShouldReturnSuccess()
        {
            var testDbActivity = TestData.CreateDbActivity("test title", "test details", 1);

            context.Activity.Add(testDbActivity);
            context.SaveChanges();
            var testActivity = mapper.Map<Models.Activity>(testDbActivity);
            testActivity.Title = "New Title!";


            var controller = new ActivitiesController(context, logger.Object, mapper);
            var result = controller.UpdateActivity(testDbActivity.Id, testActivity) as ObjectResult;

            result.Should().BeOfType<OkObjectResult>();
            result.StatusCode.Should().Be(200);
            var model = result.Value as Models.Activity;
            model.Title.Should().Be("New Title!");
            var DbActivity = context.Activity.Find(testDbActivity.Id);
            DbActivity.Title.Should().Be("New Title!");
        }
        
        [Fact]
        public void Put_ShouldReturnBadRequest()
        {
            var controller = new ActivitiesController(context, logger.Object, mapper);
            var testDbActivity = TestData.CreateDbActivity("test title", "test details", 2);
            context.Activity.Add(testDbActivity);
            context.SaveChanges();

            var result = controller.UpdateActivity(testDbActivity.Id, activity: null) as ObjectResult ;

            result.Should().BeOfType<BadRequestObjectResult>();
            result.StatusCode.Should().Be(400);
        }

        [Fact]
        public void Put_ShouldReturnNotFound()
        {
            var controller = new ActivitiesController(context, logger.Object, mapper);
            var testDbActivity = TestData.CreateDbActivity("test title", "test details", 2);

            var result = controller.UpdateActivity(testDbActivity.Id, activity: null) as ObjectResult;

            result.Should().BeOfType<NotFoundObjectResult>();
            result.StatusCode.Should().Be(404);
        }
    }
}
