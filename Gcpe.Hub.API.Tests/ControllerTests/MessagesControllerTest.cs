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
    public class MessagesControllerTests
    {
        private Mock<ILogger<MessagesController>> logger;
        private HubDbContext context;
        private IMapper mapper;

        public MessagesControllerTests()
        {
            var options = new DbContextOptionsBuilder<HubDbContext>()
                      .UseInMemoryDatabase(Guid.NewGuid().ToString())
                      .Options;
            this.context = new HubDbContext(options);

            this.logger = new Mock<ILogger<MessagesController>>();

            var mockMapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new MappingProfile());
            });
            this.mapper = mockMapper.CreateMapper();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(3)]
        public void GetAll_ShouldReturnSuccess(int messageCount)
        {
            for (var i = 0; i < messageCount; i++)
            {
                context.Message.Add(TestData.TestMessage(i.ToString()));
            }
            context.SaveChanges();
            var controller = new MessagesController(context, logger.Object, mapper);

            var result = controller.GetAll();

            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            var models = okResult.Value as ICollection<Message>;
            models.Should().NotBeNull();
            models.Count().Should().Equals(messageCount);
        }

        [Fact]
        public void GetAll_ShouldDefaultIsPublishedParameterTrue()
        {
            var publishedCount = 3;
            var unpublishedCount = 2;
            for (var i = 0; i < publishedCount; i++)
            {
                context.Message.Add(TestData.TestMessage($"published-{i.ToString()}"));
            }
            for (var i = 0; i < unpublishedCount; i++)
            {
                var testMessage = TestData.TestMessage($"unpublished-{i.ToString()}");
                testMessage.IsPublished = false;
                context.Message.Add(testMessage);
            }
            context.SaveChanges();
            var controller = new MessagesController(context, logger.Object, mapper);

            var result = controller.GetAll();

            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var models = okResult.Value as ICollection<Message>;
            models.Count().Should().Equals(publishedCount);
        }

        [Fact]
        public void GetAll_ShouldAcceptIsPublishedParameterTrue()
        {
            var publishedCount = 3;
            var unpublishedCount = 2;
            for (var i = 0; i < publishedCount; i++)
            {
                context.Message.Add(TestData.TestMessage($"published-{i.ToString()}"));
            }
            for (var i = 0; i < unpublishedCount; i++)
            {
                var testMessage = TestData.TestMessage($"unpublished-{i.ToString()}");
                testMessage.IsPublished = false;
                context.Message.Add(testMessage);
            }
            context.SaveChanges();
            var controller = new MessagesController(context, logger.Object, mapper);

            var result = controller.GetAll(true);

            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var models = okResult.Value as ICollection<Message>;
            models.Count().Should().Equals(publishedCount);
        }

        [Fact]
        public void GetAll_ShouldAcceptIsPublishedParameterFalse()
        {
            var publishedCount = 3;
            var unpublishedCount = 2;
            for (var i = 0; i < publishedCount; i++)
            {
                context.Message.Add(TestData.TestMessage($"published-{i.ToString()}"));
            }
            for (var i = 0; i < unpublishedCount; i++)
            {
                var testMessage = TestData.TestMessage($"unpublished-{i.ToString()}");
                testMessage.IsPublished = false;
                context.Message.Add(testMessage);
            }
            context.SaveChanges();
            var controller = new MessagesController(context, logger.Object, mapper);

            var result = controller.GetAll(false);

            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var models = okResult.Value as ICollection<Message>;
            models.Count().Should().Equals(unpublishedCount);
        }

        [Fact]
        public void GetAll_ShouldReturnBadRequest()
        {
            var options = new DbContextOptionsBuilder<HubDbContext>()
                      .UseInMemoryDatabase(Guid.NewGuid().ToString())
                      .Options;
            var mockContext = new Mock<HubDbContext>(options);
            mockContext.Setup(m => m.Message).Throws(new Exception());
            var controller = new MessagesController(mockContext.Object, logger.Object, mapper);

            var result = controller.GetAll();

            result.Should().BeOfType<BadRequestObjectResult>();
            var badResult = result as BadRequestObjectResult;
            badResult.StatusCode.Should().Be(400);
        }

        [Fact]
        public void Post_ShouldReturnSuccess()
        {
            var controller = new MessagesController(context, logger.Object, mapper);

            var result = controller.Post(mapper.Map<Message, MessageViewModel>(TestData.TestMessage("1")));

            result.Should().BeOfType<CreatedAtRouteResult>();
            var createdResult = result as CreatedAtRouteResult;
            createdResult.StatusCode.Should().Equals(201);
            var model = createdResult.Value as MessageViewModel;
            model.Title.Should().Equals("2018MESSAGE-1");
        }

        [Fact]
        public void Post_ShouldReturnBadRequest()
        {
            var controller = new MessagesController(context, logger.Object, mapper);
            controller.ModelState.AddModelError("error", "some validation error");
            var testMessage = TestData.TestMessage("1");

            var result = controller.Post(message: null);

            result.Should().BeOfType<BadRequestObjectResult>();
            var badResult = result as BadRequestObjectResult;
            badResult.StatusCode.Should().Be(400);
        }

        [Fact]
        public void Get_ShouldReturnSuccess()
        {
            var controller = new MessagesController(context, logger.Object, mapper);
            var testMessage = TestData.TestMessage("1");
            context.Message.Add(testMessage);
            context.SaveChanges();

            var result = controller.Get(testMessage.Id);

            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.StatusCode.Should().Be(200);
            var model = okResult.Value as MessageViewModel;
            model.Title.Should().Equals("2018MESSAGE-1");
        }

        [Fact]
        public void Get_ShouldReturnFail()
        {
            var options = new DbContextOptionsBuilder<HubDbContext>()
                      .UseInMemoryDatabase(Guid.NewGuid().ToString())
                      .Options;
            var mockContext = new Mock<HubDbContext>(options);
            mockContext.Setup(m => m.Message).Throws(new Exception());
            var controller = new MessagesController(mockContext.Object, logger.Object, mapper);
            var testMessage = TestData.TestMessage("1");
            context.Message.Add(testMessage);
            context.SaveChanges();

            var result = controller.Get(testMessage.Id);

            result.Should().BeOfType<BadRequestObjectResult>();
            var badResult = result as BadRequestObjectResult;
            badResult.StatusCode.Should().Be(400);
        }

        [Fact]
        public void Get_ShouldReturnNotFound()
        {
            var controller = new MessagesController(context, logger.Object, mapper);

            var result = controller.Get(Guid.NewGuid());

            result.Should().BeOfType<NotFoundResult>();
            var notFoundResult = result as NotFoundResult;
            notFoundResult.StatusCode.Should().Be(404);
        }
        
        [Fact]
        public void Put_ShouldReturnSuccess()
        {
            var controller = new MessagesController(context, logger.Object, mapper);
            var testMessage = TestData.TestMessage("1");
            context.Message.Add(testMessage);
            context.SaveChanges();
            testMessage.Title = "New Title!";

            var result = controller.Put(testMessage.Id, mapper.Map<Message, MessageViewModel>(testMessage));

            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.StatusCode.Should().Be(200);
            var model = okResult.Value as MessageViewModel;
            model.Title.Should().Equals("New Title!");
            var dbMessage = context.Message.Find(testMessage.Id);
            dbMessage.Title.Should().Equals("New Title!");
        }

        [Fact]
        public void Put_ShouldReturnBadRequest()
        {
            var controller = new MessagesController(context, logger.Object, mapper);
            var testMessage = TestData.TestMessage("1");
            context.Message.Add(testMessage);
            context.SaveChanges();

            var result = controller.Put(testMessage.Id, message: null);

            result.Should().BeOfType<BadRequestObjectResult>();
            var badResult = result as BadRequestObjectResult;
            badResult.StatusCode.Should().Be(400);
        }

        [Fact]
        public void Put_ShouldReturnNotFound()
        {
            var controller = new MessagesController(context, logger.Object, mapper);
            var testMessage = TestData.TestMessage("1");

            var result = controller.Put(testMessage.Id, message: null);

            result.Should().BeOfType<NotFoundObjectResult>();
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult.StatusCode.Should().Be(404);
        }
    }
}
