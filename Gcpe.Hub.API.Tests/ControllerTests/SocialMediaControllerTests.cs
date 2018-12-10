using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using FluentAssertions;
using Gcpe.Hub.API.Controllers;
using Gcpe.Hub.API.Data;
using Gcpe.Hub.API.ViewModels;
using Gcpe.Hub.Data.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Gcpe.Hub.API.Tests.ControllerTests
{
    public class SocialMediaControllerTests
    {
        private Mock<ILogger<SocialMediaController>> logger;
        private HubDbContext context;
        private IMapper mapper;
        private SocialMediaController controller;

        public SocialMediaControllerTests()
        {
            var options = new DbContextOptionsBuilder<HubDbContext>()
                      .UseInMemoryDatabase(Guid.NewGuid().ToString())
                      .Options;
            this.context = new HubDbContext(options);
            this.logger = new Mock<ILogger<SocialMediaController>>();
            var mockMapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new MappingProfile());
            });
            this.mapper = mockMapper.CreateMapper();
            this.controller = new SocialMediaController(context, logger.Object, mapper);
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
                var post = TestData.CreateSocialMediaPost("http://facebook.com/post/123");
                post.Id = Guid.NewGuid();
                post.SortOrder = postCount - i;
                context.SocialMediaPost.Add(post);
            }
            context.SaveChanges();

            var result = controller.GetAll();
            var okResult = result as ObjectResult;

            okResult.Should().BeOfType<OkObjectResult>();
            okResult.Should().NotBeNull();

            var models = okResult.Value as IList<SocialMediaPostViewModel>;
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
                var post = TestData.CreateSocialMediaPost("http://facebook.com/post/123");
                post.Id = Guid.NewGuid();
                post.IsActive = false;
                context.SocialMediaPost.Add(post);
            }
            context.SaveChanges();

            var result = controller.GetAll() as ObjectResult;

            result.Should().BeOfType<OkObjectResult>();
            result.StatusCode.Should().Be(200);
            result.Should().NotBeNull();

            var models = result.Value as ICollection<SocialMediaPostViewModel>;
            models.Should().NotBeNull();
            models.Count().Should().Be(0);
        }

        [Fact]
        public void GetAll_ShouldReturnBadRequest()
        {
            var options = new DbContextOptionsBuilder<HubDbContext>()
                      .UseInMemoryDatabase(Guid.NewGuid().ToString())
                      .Options;
            var mockContext = new Mock<HubDbContext>(options);
            mockContext.Setup(m => m.SocialMediaPost).Throws(new Exception());
            var controller = new SocialMediaController(mockContext.Object, logger.Object, mapper);

            var result = controller.GetAll() as ObjectResult;

            result.Should().BeOfType<BadRequestObjectResult>();
            result.StatusCode.Should().Be(400);
        }

        [Fact]
        public void Post_ShouldReturnSuccess()
        {
            var result = controller.Post(postVM: mapper.Map<SocialMediaPost, SocialMediaPostViewModel>(TestData.CreateSocialMediaPost("http://facebook.com/post/123")));
            var createdResult = result as ObjectResult;

            createdResult.Should().BeOfType<CreatedAtRouteResult>();
            createdResult.StatusCode.Should().Be(201);

            var model = createdResult.Value as SocialMediaPostViewModel;
            model.Url.Should().Be(TestData.CreateSocialMediaPost("http://facebook.com/post/123").Url);
        }


        [Fact]
        public void Post_ShouldReturnBadRequest()
        {
            controller.ModelState.AddModelError("error", "some validation error");
            var testSocialMediaPost = TestData.CreateSocialMediaPost("http://facebook.com/post/123");

            var result = controller.Post(postVM: null) as ObjectResult;

            result.Should().BeOfType<BadRequestObjectResult>();
            result.StatusCode.Should().Be(400);
        }

        [Fact]
        public void Get_ShouldReturnSuccess()
        {
            var testSocialMediaPost = TestData.CreateSocialMediaPost("http://facebook.com/post/123");
            context.SocialMediaPost.Add(testSocialMediaPost);
            context.SaveChanges();

            var result = controller.Get(testSocialMediaPost.Id) as ObjectResult;

            result.Should().BeOfType<OkObjectResult>();
            result.StatusCode.Should().Be(200);

            var model = result.Value as SocialMediaPostViewModel;
            model.Url.Should().Be(testSocialMediaPost.Url);
        }

        [Fact]
        public void Get_ShouldntReturnDeletedPost()
        {
            var testSocialMediaPost = TestData.CreateSocialMediaPost("http://facebook.com/post/123");
            testSocialMediaPost.IsActive = false;
            context.SocialMediaPost.Add(testSocialMediaPost);
            context.SaveChanges();

            var result = controller.Get(testSocialMediaPost.Id) as ObjectResult;

            result.Should().BeOfType<NotFoundObjectResult>();
            result.StatusCode.Should().Be(404);
        }

        [Fact]
        public void Get_ShouldReturnFail()
        {
            var options = new DbContextOptionsBuilder<HubDbContext>()
                      .UseInMemoryDatabase(Guid.NewGuid().ToString())
                      .Options;
            var mockContext = new Mock<HubDbContext>(options);
            mockContext.Setup(m => m.SocialMediaPost).Throws(new Exception());
            var controller = new SocialMediaController(mockContext.Object, logger.Object, mapper);

            var testSocialMediaPost = TestData.CreateSocialMediaPost("http://facebook.com/post/123");
            context.SocialMediaPost.Add(testSocialMediaPost);
            context.SaveChanges();
       
            var result = controller.Get(testSocialMediaPost.Id) as ObjectResult;

            result.Should().BeOfType<BadRequestObjectResult>();
            result.StatusCode.Should().Be(400);
        }

        [Fact]
        public void Get_ShouldReturnNotFound()
        {
            var result = controller.Get(Guid.NewGuid()) as ObjectResult;

            result.Should().BeOfType<NotFoundObjectResult>();
            result.StatusCode.Should().Be(404);
        }

        [Fact]
        public void Put_ShouldReturnSuccess()
        {
            var testPost = TestData.CreateSocialMediaPost("http://facebook.com/post/123");

            context.SocialMediaPost.Add(testPost);
            context.SaveChanges();
            var socialMediaPostVM = mapper.Map<SocialMediaPost, SocialMediaPostViewModel>(testPost);
            socialMediaPostVM.Url = "http://twitter.com/post/123";

            var result = controller.Put(testPost.Id, socialMediaPostVM) as ObjectResult;

            result.Should().BeOfType<OkObjectResult>();
            result.StatusCode.Should().Be(200);
            var model = result.Value as SocialMediaPostViewModel;
            model.Url.Should().Be("http://twitter.com/post/123");
            var dbMessage = context.SocialMediaPost.Find(testPost.Id);
            dbMessage.Url.Should().Be("http://twitter.com/post/123");
        }

        [Fact]
        public void Put_ShouldntUpdateDeletedPost()
        {
            var testPost = TestData.CreateSocialMediaPost("http://facebook.com/post/123");
            testPost.IsActive = false;
            context.SocialMediaPost.Add(testPost);
            context.SaveChanges();
            var socialMediaPostVM = mapper.Map<SocialMediaPost, SocialMediaPostViewModel>(testPost);
            socialMediaPostVM.Url = "http://twitter.com/post/123";

            var result = controller.Put(testPost.Id, socialMediaPostVM) as ObjectResult;

            result.Should().BeOfType<NotFoundObjectResult>();
            result.StatusCode.Should().Be(404);
        }

        [Fact]
        public void Put_ShouldReturnBadRequest()
        {
            var testSocialMediaPost = TestData.CreateSocialMediaPost("http://facebook.com/post/123");
            context.SocialMediaPost.Add(testSocialMediaPost);
            context.SaveChanges();

            var result = controller.Put(testSocialMediaPost.Id, postVM: null) as ObjectResult;

            result.Should().BeOfType<BadRequestObjectResult>();
            result.StatusCode.Should().Be(400);
        }

        [Fact]
        public void Put_ShouldReturnNotFound()
        {
            var result = controller.Put(Guid.NewGuid(), postVM: null) as ObjectResult;

            result.Should().BeOfType<NotFoundObjectResult>();
            result.StatusCode.Should().Be(404);
        }

        [Fact]
        public void Delete_ShouldReturnSuccess()
        {
            var testPost = TestData.CreateSocialMediaPost("http://facebook.com/post/123");
            testPost.Id = Guid.NewGuid();
            context.SocialMediaPost.Add(testPost);
            context.SaveChanges();

            var result = controller.Delete(testPost.Id) as StatusCodeResult;

            result.Should().BeOfType<NoContentResult>();
            result.StatusCode.Should().Be(204);
        }

        [Fact]
        public void Delete_ShouldReturnBadRequest()
        {
            var options = new DbContextOptionsBuilder<HubDbContext>()
                      .UseInMemoryDatabase(Guid.NewGuid().ToString())
                      .Options;
            var mockContext = new Mock<HubDbContext>(options);
            mockContext.Setup(m => m.SocialMediaPost).Throws(new Exception());
            var controller = new SocialMediaController(mockContext.Object, logger.Object, mapper);

            var result = controller.Delete(Guid.NewGuid()) as ObjectResult;

            result.Should().BeOfType<BadRequestObjectResult>();
            result.StatusCode.Should().Be(400);
        }

        [Fact]
        public void Delete_ShouldReturnNotFound()
        {
            var result = controller.Delete(Guid.NewGuid()) as ObjectResult;

            result.Should().BeOfType<NotFoundObjectResult>();
            result.StatusCode.Should().Be(404);
        }
    }
}
