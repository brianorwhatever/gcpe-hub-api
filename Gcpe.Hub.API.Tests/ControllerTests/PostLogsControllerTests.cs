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
    public class PostLogsControllerTests
    {
        private Mock<ILogger<PostLogsController>> logger;
        private HubDbContext context;
        private IMapper mapper;
        private PostLogsController controller;
        private DbContextOptions<HubDbContext> options;

        public PostLogsControllerTests()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            options = new DbContextOptionsBuilder<HubDbContext>()
                      .UseInMemoryDatabase(Guid.NewGuid().ToString())
                      .Options;
            context = new HubDbContext(options);
            logger = new Mock<ILogger<PostLogsController>>();
            var mockMapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new MappingProfile());
            });
            mapper = mockMapper.CreateMapper();
            controller = new PostLogsController(context, logger.Object, mapper);
        }

        [Fact]
        public void GetPostLogs_ShouldReturnSuccess()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            NewsRelease post = TestData.CreateDbPost();
            context.NewsRelease.Add(post);
            post.NewsReleaseLog = TestData.CreateDbPostLogs(post);
            var expectedLogEntry = post.NewsReleaseLog.FirstOrDefault();
            var expectedCount = post.NewsReleaseLog.Count;
            context.SaveChanges();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = controller.GetPostLogs(post.Key) as ObjectResult;

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().BeOfType(typeof(OkObjectResult), "because the read operation should go smoothly");
            var actual = result.Value as IEnumerable<Models.PostLog>;
            var actualLogEntry = actual.FirstOrDefault();

            actual.Count().Should().Be(expectedCount);
            actualLogEntry.PostKey.Should().Be(expectedLogEntry.Release.Key);
            actualLogEntry.Description.Should().Be(expectedLogEntry.Description);
        }

        [Fact]
        public void GetPostLogs_ShouldReturnNotFound_WhenGivenInvalidReleaseId()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = controller.GetPostLogs("-1");  // does not exist...

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().BeOfType(typeof(NotFoundResult), "because an invalid Key should not yield a result");
        }

        [Fact]
        public void Post_ShouldReturnSuccess()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            NewsRelease post = TestData.CreateDbPost();
            context.NewsRelease.Add(post);
            context.SaveChanges();

            var releaseLogToCreate = new Models.PostLog
            {
                Description = "toto",
                DateTime = DateTime.Now,
                PostKey = post.Key
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = controller.AddPostLog(releaseLogToCreate) as ObjectResult;

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().BeOfType<CreatedAtRouteResult>("because the create operation should go smoothly");
            result.StatusCode.Should().Be(201, "because HTTP Status 201 should be returned upon creation of new entry");
            var model = result.Value as Models.PostLog;
            model.Description.Should().Be("toto");

        }
    }
}
