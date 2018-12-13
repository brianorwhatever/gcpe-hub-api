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
    public class SocialMediaPostsControllerTests
    {
        private Mock<ILogger<SocialMediaPostsController>> logger;
        private HubDbContext context;
        private IMapper mapper;
        private SocialMediaPostsController controller;
        private DbContextOptions<HubDbContext> options;

        public SocialMediaPostsControllerTests()
        {
            options = new DbContextOptionsBuilder<HubDbContext>()
                      .UseInMemoryDatabase(Guid.NewGuid().ToString())
                      .Options;
            context = new HubDbContext(options);
            logger = new Mock<ILogger<SocialMediaPostsController>>();
            var mockMapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new MappingProfile());
            });
            mapper = mockMapper.CreateMapper();
            controller = new SocialMediaPostsController(context, logger.Object, mapper);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(3)]
        [InlineData(10)]
        public void GetAll_ShouldReturnSuccessAndSorted(int postCount)
        {
            for (var i = 0; i < postCount; i++)
            {
                context.SocialMediaPost.Add(TestData.CreateDbSocialMediaPost("http://facebook.com/post/123", Guid.NewGuid(), true, postCount - i));
            }
            context.SaveChanges();

            var result = controller.GetAllSocialMediaPosts();
            var okResult = result as ObjectResult;

            okResult.Should().BeOfType<OkObjectResult>();
            okResult.Should().NotBeNull();

            var models = okResult.Value as IList<Models.SocialMediaPost>;
            models.Should().NotBeNull();
            models.Count().Should().Be(postCount);
            for (int i = 0; i < models.Count() - 1; i++)
            {
                models[i].SortOrder.Should().BeLessThan(models[i + 1].SortOrder);
            }
        }

        [Fact]
        public void GetAll_ShouldntReturnDeletePosts()
        {
            for (var i = 0; i < 3; i++)
            {
                context.SocialMediaPost.Add(TestData.CreateDbSocialMediaPost("http://facebook.com/post/123", Guid.NewGuid(), false));
            }
            context.SaveChanges();

            var result = controller.GetAllSocialMediaPosts() as ObjectResult;

            result.Should().BeOfType<OkObjectResult>();
            result.StatusCode.Should().Be(200);
            result.Should().NotBeNull();

            var models = result.Value as ICollection<Models.SocialMediaPost>;
            models.Should().NotBeNull();
            models.Count().Should().Be(0);
        }

        [Fact]
        public void GetAll_ShouldReturnBadRequest()
        {
            var mockContext = new Mock<HubDbContext>(options);
            mockContext.Setup(m => m.SocialMediaPost).Throws(new Exception());
            var controller = new SocialMediaPostsController(mockContext.Object, logger.Object, mapper);

            var result = controller.GetAllSocialMediaPosts() as ObjectResult;

            result.Should().BeOfType<BadRequestObjectResult>();
            result.StatusCode.Should().Be(400);
        }

        [Fact]
        public void Post_ShouldReturnSuccess()
        {
            var testPost = TestData.CreateDbSocialMediaPost("http://facebook.com/post/123");
            var createdResult = controller.AddSocialMediaPost(socialMediaPost: mapper.Map<Models.SocialMediaPost>(testPost)) as ObjectResult;

            createdResult.Should().BeOfType<CreatedAtRouteResult>();
            createdResult.StatusCode.Should().Be(201);

            var model = createdResult.Value as Models.SocialMediaPost;
            model.Url.Should().Be(testPost.Url);
        }


        [Fact]
        public void Post_ShouldReturnBadRequest()
        {
            controller.ModelState.AddModelError("error", "some validation error");
            var testDbPost = TestData.CreateDbSocialMediaPost("http://facebook.com/post/123");

            var result = controller.AddSocialMediaPost(socialMediaPost: null) as ObjectResult;

            result.Should().BeOfType<BadRequestObjectResult>();
            result.StatusCode.Should().Be(400);
        }

        [Fact]
        public void Get_ShouldReturnSuccess()
        {
            var testDbPost = TestData.CreateDbSocialMediaPost("http://facebook.com/post/123");
            context.SocialMediaPost.Add(testDbPost);
            context.SaveChanges();

            var result = controller.GetSocialMediaPost(testDbPost.Id) as ObjectResult;

            result.Should().BeOfType<OkObjectResult>();
            result.StatusCode.Should().Be(200);

            var model = result.Value as Models.SocialMediaPost;
            model.Url.Should().Be(testDbPost.Url);
        }

        [Fact]
        public void Get_ShouldntReturnDeletedPost()
        {
            var testDbPost = TestData.CreateDbSocialMediaPost("http://facebook.com/post/123");
            testDbPost.IsActive = false;
            context.SocialMediaPost.Add(testDbPost);
            context.SaveChanges();

            var result = controller.GetSocialMediaPost(testDbPost.Id) as ObjectResult;

            result.Should().BeOfType<NotFoundObjectResult>();
            result.StatusCode.Should().Be(404);
        }

        [Fact]
        public void Get_ShouldReturnFail()
        {
            var mockContext = new Mock<HubDbContext>(options);
            mockContext.Setup(m => m.SocialMediaPost).Throws(new Exception());
            var controller = new SocialMediaPostsController(mockContext.Object, logger.Object, mapper);

            var testDbPost = TestData.CreateDbSocialMediaPost("http://facebook.com/post/123");
            context.SocialMediaPost.Add(testDbPost);
            context.SaveChanges();
       
            var result = controller.GetSocialMediaPost(testDbPost.Id) as ObjectResult;

            result.Should().BeOfType<BadRequestObjectResult>();
            result.StatusCode.Should().Be(400);
        }

        [Fact]
        public void Get_ShouldReturnNotFound()
        {
            var result = controller.GetSocialMediaPost(Guid.NewGuid()) as ObjectResult;

            result.Should().BeOfType<NotFoundObjectResult>();
            result.StatusCode.Should().Be(404);
        }

        [Fact]
        public void Put_ShouldReturnSuccess()
        {
            var testDbPost = TestData.CreateDbSocialMediaPost("http://facebook.com/post/123");

            context.SocialMediaPost.Add(testDbPost);
            context.SaveChanges();
            var socialMediaPostModel = mapper.Map<Models.SocialMediaPost>(testDbPost);
            socialMediaPostModel.Url = "http://twitter.com/post/123";

            var result = controller.UpdateSocialMediaPost(testDbPost.Id, socialMediaPostModel) as ObjectResult;

            result.Should().BeOfType<OkObjectResult>();
            result.StatusCode.Should().Be(200);
            var model = result.Value as Models.SocialMediaPost;
            model.Url.Should().Be("http://twitter.com/post/123");
            var dbMessage = context.SocialMediaPost.Find(testDbPost.Id);
            dbMessage.Url.Should().Be("http://twitter.com/post/123");
        }

        [Fact]
        public void Put_ShouldntUpdateDeletedPost()
        {
            var testDbPost = TestData.CreateDbSocialMediaPost("http://facebook.com/post/123", null, false);
            context.SocialMediaPost.Add(testDbPost);
            context.SaveChanges();
            var socialMediaPostModel = mapper.Map<Models.SocialMediaPost>(testDbPost);
            socialMediaPostModel.Url = "http://twitter.com/post/123";

            var result = controller.UpdateSocialMediaPost(testDbPost.Id, socialMediaPostModel) as ObjectResult;

            result.Should().BeOfType<NotFoundObjectResult>();
            result.StatusCode.Should().Be(404);
        }

        [Fact]
        public void Put_ShouldReturnBadRequest()
        {
            var testDbPost = TestData.CreateDbSocialMediaPost("http://facebook.com/post/123");
            context.SocialMediaPost.Add(testDbPost);
            context.SaveChanges();

            var result = controller.UpdateSocialMediaPost(testDbPost.Id, socialMediaPost: null) as ObjectResult;

            result.Should().BeOfType<BadRequestObjectResult>();
            result.StatusCode.Should().Be(400);
        }

        [Fact]
        public void Put_ShouldReturnNotFound()
        {
            var result = controller.UpdateSocialMediaPost(Guid.NewGuid(), socialMediaPost: null) as ObjectResult;

            result.Should().BeOfType<NotFoundObjectResult>();
            result.StatusCode.Should().Be(404);
        }

        [Fact]
        public void Delete_ShouldReturnSuccess()
        {
            var testDbPost = TestData.CreateDbSocialMediaPost("http://facebook.com/post/123", Guid.NewGuid());
            context.SocialMediaPost.Add(testDbPost);
            context.SaveChanges();

            var result = controller.DeleteSocialMediaPost(testDbPost.Id) as StatusCodeResult;

            result.Should().BeOfType<NoContentResult>();
            result.StatusCode.Should().Be(204);
        }

        [Fact]
        public void Delete_ShouldReturnBadRequest()
        {
            var mockContext = new Mock<HubDbContext>(options);
            mockContext.Setup(m => m.SocialMediaPost).Throws(new Exception());
            var controller = new SocialMediaPostsController(mockContext.Object, logger.Object, mapper);

            var result = controller.DeleteSocialMediaPost(Guid.NewGuid()) as ObjectResult;

            result.Should().BeOfType<BadRequestObjectResult>();
            result.StatusCode.Should().Be(400);
        }

        [Fact]
        public void Delete_ShouldReturnNotFound()
        {
            var result = controller.DeleteSocialMediaPost(Guid.NewGuid()) as ObjectResult;

            result.Should().BeOfType<NotFoundObjectResult>();
            result.StatusCode.Should().Be(404);
        }
    }
}
