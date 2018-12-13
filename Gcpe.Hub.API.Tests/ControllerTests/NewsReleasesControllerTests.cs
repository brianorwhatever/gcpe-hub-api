using System;
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
    public class NewsReleasesControllerTests
    {
        private Mock<ILogger<NewsReleasesController>> logger;
        private HubDbContext context;
        private IMapper mapper;
        private NewsReleasesController controller;
        private DbContextOptions<HubDbContext> options;

        public NewsReleasesControllerTests()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            options = new DbContextOptionsBuilder<HubDbContext>()
                      .UseInMemoryDatabase(Guid.NewGuid().ToString())
                      .Options;
            context = new HubDbContext(options);
            logger = new Mock<ILogger<NewsReleasesController>>();
            var mockMapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new MappingProfile());
            });
            mapper = mockMapper.CreateMapper();
            controller = new NewsReleasesController(context, logger.Object, mapper);
        }

        [Fact]
        public void GetByKey_ShouldReturnSuccess()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var _expectedModelReturn = TestData.CreateDbPost();
            context.NewsRelease.Add(_expectedModelReturn);
            context.SaveChanges();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = controller.GetPost(_expectedModelReturn.Key);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().BeOfType(typeof(OkObjectResult), "because the read operation should go smoothly");

            var actualValue = ((result as OkObjectResult).Value as Models.NewsRelease);
            actualValue.Key.Should().Be(_expectedModelReturn.Key);
            actualValue.PublishDateTime.Should().Be(_expectedModelReturn.PublishDateTime);
            actualValue.Summary.Should().Be(_expectedModelReturn.NewsReleaseLanguage.First().Summary);
        }

        [Fact]
        public void GetByKey_ShouldReturnNotFound_WhenGivenInvalidId()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = controller.GetPost("-1");  // does not exist...

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().BeOfType(typeof(NotFoundObjectResult), "because an invalid Key should not yield a result");
        }

        [Fact]
        public void GetByKey_ShouldReturnBadRequest_WhenDataSourceIsUnavailable()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var validId = "0";
            var mockContext = new Mock<HubDbContext>(options);
            mockContext.Setup(m => m.NewsRelease).Throws(new InvalidOperationException());
            var controller = new NewsReleasesController(mockContext.Object, logger.Object, mapper);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = controller.GetPost(validId) as BadRequestObjectResult;

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().BeOfType(typeof(BadRequestObjectResult), "because the read operation should require a valid data source");
            result.StatusCode.Should().Be(400, "because HTTP Status 400 should be returned to signal a Bad Request");
        }

        [Fact]
        public void GetAllNewsReleases_ShouldReturnSuccess()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var paginationParams = new NewsReleaseParams();
            var expectedReleasesPerPage = paginationParams.PageSize;
            for (var i = 0; i < expectedReleasesPerPage; i++)
            {
                context.NewsRelease.Add(TestData.CreateDbPost($"2018PREM{i}-{i}00000"));
            }
            context.SaveChanges();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var results = controller.GetResultsPage(paginationParams);
            var actualNumberOfReleases = results.Count();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            actualNumberOfReleases.Should().Be(expectedReleasesPerPage);
        }

        [Fact]
        public void GetAllNewsReleases_ShouldReturnBadRequest_WhenDataSourceIsUnavailable()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var mockContext = new Mock<HubDbContext>(options);
            mockContext.Setup(m => m.NewsRelease).Throws(new InvalidOperationException());
            var controller = new NewsReleasesController(mockContext.Object, logger.Object, mapper);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = controller.GetAll(newsReleaseParams: new NewsReleaseParams()) as BadRequestObjectResult;

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().BeOfType(typeof(BadRequestObjectResult), "because the read operation should require a valid data source");
            result.StatusCode.Should().Be(400, "because HTTP Status 400 should be returned to signal a Bad Request");
        }

        [Fact]
        public void Post_ShouldCreateNewPostAndReturnSuccess()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var releaseToCreate = new Models.NewsRelease
            {
                Summary = "toto",
                Kind = "Release",
                PublishDateTime = DateTime.Now
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = controller.AddPost(releaseToCreate) as ObjectResult;

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().BeOfType<CreatedAtRouteResult>("because the create operation should go smoothly");
            result.StatusCode.Should().Be(201, "because HTTP Status 201 should be returned upon creation of new entity");
            var model = result.Value as Models.NewsRelease;
            model.Summary.Should().Be(releaseToCreate.Summary);
            model.Kind.Should().Be(releaseToCreate.Kind);


            // this will throw if the System-Under-Test (SUT) i.e. the controller didn't call repository.AddEntity(...)
            //mockRepository.Verify();
        }

        [Fact]
        public void Post_ShouldReturnBadRequest_WhenGivenInvalidModel()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            controller.ModelState.AddModelError("error", "some validation error");

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = controller.AddPost(post: null) as BadRequestObjectResult;

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().BeOfType(typeof(BadRequestObjectResult), "because the create operation should not work with invalid data");
            result.StatusCode.Should().Be(400, "because HTTP Status 400 should be returned to signal a Bad Request");
        }

        [Fact]
        public void Put_ShouldUpdateEntityAndReturnSuccess()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var dbPost = TestData.CreateDbPost();
            context.NewsRelease.Add(dbPost);
            context.SaveChanges();
            Models.NewsRelease expectedModelReturn = dbPost.ToModel(mapper);
            expectedModelReturn.Kind = "Release";
            expectedModelReturn.Summary = "toto";

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------

            var result = controller.UpdatePost(dbPost.Key, expectedModelReturn) as ObjectResult;

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().BeOfType(typeof(OkObjectResult), "because the update operation should go smoothly");
            var model = result.Value as Models.NewsRelease;
            model.Summary.Should().Be(expectedModelReturn.Summary);
            model.Kind.Should().Be(expectedModelReturn.Kind);
        }

        [Fact]
        public void Put_ShouldReturnNotFound_WhenGivenInvalidId()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            Models.NewsRelease testPost = TestData.CreateDbPost("0").ToModel(mapper);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = controller.UpdatePost("-1", testPost); // -1 does not exist...

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().BeOfType(typeof(NotFoundObjectResult), "because a valid key is required to update a post");
        }

        [Fact]
        public void Put_ShouldReturnBadRequest_WhenDataSourceIsUnavailable()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var mockContext = new Mock<HubDbContext>(options);
            mockContext.Setup(m => m.NewsRelease).Throws(new InvalidOperationException());
            var controller = new NewsReleasesController(mockContext.Object, logger.Object, mapper);
            Models.NewsRelease testPost = TestData.CreateDbPost().ToModel(mapper);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = controller.UpdatePost(testPost.Key, testPost) as ObjectResult;

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().BeOfType(typeof(BadRequestObjectResult), "because the update operation should require a valid data source");
            result.StatusCode.Should().Be(400, "because HTTP Status 400 should be returned to signal a Bad Request");
        }
    }
}
