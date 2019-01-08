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
    public class PostsControllerTests
    {
        private Mock<ILogger<PostsController>> logger;
        private HubDbContext context;
        private IMapper mapper;
        private DbContextOptions<HubDbContext> options;

        public PostsControllerTests()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            options = new DbContextOptionsBuilder<HubDbContext>()
                      .UseInMemoryDatabase(Guid.NewGuid().ToString())
                      .Options;
            context = new HubDbContext(options);
            logger = new Mock<ILogger<PostsController>>();
            var mockMapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new MappingProfile());
            });
            mapper = mockMapper.CreateMapper();
        }

        private PostsController Controller(HubDbContext c = null)
        {
            return new PostsController(c ?? context, logger.Object, mapper, null);
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
            var result = Controller().GetPost(_expectedModelReturn.Key);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().BeOfType(typeof(OkObjectResult), "because the read operation should go smoothly");

            var actualValue = ((result as OkObjectResult).Value as Models.Post);
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
            var result = Controller().GetPost("-1");  // does not exist...

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

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = Controller(mockContext.Object).GetPost(validId) as BadRequestObjectResult;

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().BeOfType(typeof(BadRequestObjectResult), "because the read operation should require a valid data source");
            result.StatusCode.Should().Be(400, "because HTTP Status 400 should be returned to signal a Bad Request");
        }

        [Fact]
        public void GetAllPosts_ShouldReturnSuccess()
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
            int actualNumberOfReleases;
            var results = Controller().GetResultsPage(paginationParams, out actualNumberOfReleases);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            actualNumberOfReleases.Should().Be(expectedReleasesPerPage);
        }

        [Fact]
        public void GetAllPosts_ShouldReturnBadRequest_WhenDataSourceIsUnavailable()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var mockContext = new Mock<HubDbContext>(options);
            mockContext.Setup(m => m.NewsRelease).Throws(new InvalidOperationException());

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = Controller(mockContext.Object).GetAllPosts(postParams: new NewsReleaseParams()) as BadRequestObjectResult;

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
            var releaseToCreate = new Models.Post
            {
                Summary = "toto",
                Kind = "Release",
                PublishDateTime = DateTime.Now
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = Controller().AddPost(releaseToCreate) as ObjectResult;

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().BeOfType<CreatedAtRouteResult>("because the create operation should go smoothly");
            result.StatusCode.Should().Be(201, "because HTTP Status 201 should be returned upon creation of new post");
            var model = result.Value as Models.Post;
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
            PostsController controller = Controller();
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
            Models.Post expectedModelReturn = dbPost.ToModel(mapper);
            expectedModelReturn.Kind = "Release";
            expectedModelReturn.Summary = "toto";

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------

            var result = Controller().UpdatePost(dbPost.Key, expectedModelReturn) as ObjectResult;

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().BeOfType(typeof(OkObjectResult), "because the update operation should go smoothly");
            var model = result.Value as Models.Post;
            model.Summary.Should().Be(expectedModelReturn.Summary);
            model.Kind.Should().Be(expectedModelReturn.Kind);
        }

        [Fact]
        public void Put_ShouldReturnNotFound_WhenGivenInvalidId()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            Models.Post testPost = TestData.CreateDbPost("0").ToModel(mapper);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = Controller().UpdatePost("-1", testPost); // -1 does not exist...

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
            Models.Post testPost = TestData.CreateDbPost().ToModel(mapper);

            var mockContext = new Mock<HubDbContext>(options);
            mockContext.Setup(m => m.NewsRelease).Throws(new InvalidOperationException());
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = Controller(mockContext.Object).UpdatePost(testPost.Key, testPost) as ObjectResult;

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().BeOfType(typeof(BadRequestObjectResult), "because the update operation should require a valid data source");
            result.StatusCode.Should().Be(400, "because HTTP Status 400 should be returned to signal a Bad Request");
        }
    }
}
