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
    public class MessagesControllerTests : IDisposable
    {
        private Mock<ILogger<MessagesController>> logger;
        private HubDbContext context;
        private IMapper mapper;
        private DbContextOptions<HubDbContext> options;

        public MessagesControllerTests()
        {
            this.options = new DbContextOptionsBuilder<HubDbContext>()
                      .UseInMemoryDatabase(Guid.NewGuid().ToString())
                      .Options;
            this.context = new HubDbContext(this.options);

            this.logger = new Mock<ILogger<MessagesController>>();

            var mockMapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new MappingProfile());
            });
            this.mapper = mockMapper.CreateMapper();
        }
        public void Dispose()
        {
            this.context.Dispose();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(3)]
        public void GetAll_ShouldReturnSuccess(int messageCount)
        {
            for (var i = 0; i < messageCount; i++)
            {
                var message = TestData.CreateMessage(i.ToString(), "test description", 0, true, false);
                message.Id = Guid.NewGuid();
                context.Message.Add(message);
            }
            context.SaveChanges();
            var controller = new MessagesController(context, logger.Object, mapper);

            var result = controller.GetAll() as ObjectResult;

            result.Should().BeOfType<OkObjectResult>();
            result.Should().NotBeNull();
            var models = result.Value as ICollection<Message>;
            models.Should().NotBeNull();
            models.Count().Should().Be(messageCount);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GetAll_ShouldDefaultIsPublishedParameterTrue(bool isPublished)
        {
            var publishedCount = 3;
            var unpublishedCount = 2;
            for (var i = 0; i < publishedCount; i++)
            {
                var testMessage = TestData.CreateMessage($"published-{i.ToString()}", "test description", 0, true, false);
                testMessage.Id = Guid.NewGuid();
                context.Message.Add(testMessage);
            }
            for (var i = 0; i < unpublishedCount; i++)
            {
                var testMessage = TestData.CreateMessage($"unpublished-{i.ToString()}", "test description", 0, false, false);
                testMessage.Id = Guid.NewGuid();
                context.Message.Add(testMessage);
            }
            context.SaveChanges();
            var controller = new MessagesController(context, logger.Object, mapper);

            var result = controller.GetAll(isPublished) as ObjectResult;

            result.Should().BeOfType<OkObjectResult>();
            var models = result.Value as ICollection<Message>;
            models.Count().Should().Be(isPublished ? publishedCount : unpublishedCount);
        }

        [Fact]
        public void GetAll_ShouldReturnBadRequest()
        {
            var mockContext = new Mock<HubDbContext>(this.options);
            mockContext.Setup(m => m.Message).Throws(new Exception());
            var controller = new MessagesController(mockContext.Object, logger.Object, mapper);

            var result = controller.GetAll() as ObjectResult;

            result.StatusCode.Should().Be(400);
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public void Post_ShouldReturnSuccess()
        {
            var controller = new MessagesController(context, logger.Object, mapper);
            var testMessage = TestData.CreateMessage("2018MESSAGE-1", "test description", 0);

            var result = controller.Post(mapper.Map<Message, MessageViewModel>(testMessage)) as ObjectResult;

            result.StatusCode.Should().Be(201);
            result.Should().BeOfType<CreatedAtRouteResult>();
            var model = result.Value as MessageViewModel;
            model.Title.Should().Be("2018MESSAGE-1");
        }

        [Fact]
        public void Post_ShouldReturnBadRequest()
        {
            var controller = new MessagesController(context, logger.Object, mapper);
            var testMessage = TestData.CreateMessage("1", "test description", 0);
            testMessage.Id = Guid.NewGuid();

            var result = controller.Post(mapper.Map<Message, MessageViewModel>(testMessage)) as ObjectResult;

            result.Should().BeOfType<BadRequestObjectResult>();
            result.StatusCode.Should().Be(400);
        }

        [Fact]
        public void Get_ShouldReturnSuccess()
        {
            var controller = new MessagesController(context, logger.Object, mapper);
            var testMessage = TestData.CreateMessage("2018MESSAGE-1", "test description", 0);
            context.Message.Add(testMessage);
            context.SaveChanges();

            var result = controller.Get(testMessage.Id) as ObjectResult;

            result.Should().BeOfType<OkObjectResult>();
            result.StatusCode.Should().Be(200);
            var model = result.Value as MessageViewModel;
            model.Title.Should().Be("2018MESSAGE-1");
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
            var testMessage = TestData.CreateMessage("1", "test description", 0);
            context.Message.Add(testMessage);
            context.SaveChanges();

            var result = controller.Get(testMessage.Id) as ObjectResult;

            result.Should().BeOfType<BadRequestObjectResult>();
            result.StatusCode.Should().Be(400);
        }

        [Fact]
        public void Get_ShouldReturnNotFound()
        {
            var controller = new MessagesController(context, logger.Object, mapper);

            var result = controller.Get(Guid.NewGuid()) as ObjectResult;

            result.Should().BeOfType<NotFoundObjectResult>();
            result.StatusCode.Should().Be(404);
        }
        
        [Fact]
        public void Put_ShouldReturnSuccess()
        {
            var testMessage = TestData.CreateMessage("1", "test description", 0);

            context.Message.Add(testMessage);
            context.SaveChanges();
            var testMessageVM = mapper.Map<Message, MessageViewModel>(testMessage);
            testMessageVM.Title = "New Title!";


            var controller = new MessagesController(context, logger.Object, mapper);
            var result = controller.Put(testMessage.Id, testMessageVM) as ObjectResult;

            result.Should().BeOfType<OkObjectResult>();
            result.StatusCode.Should().Be(200);
            var model = result.Value as MessageViewModel;
            model.Title.Should().Be("New Title!");
            var dbMessage = context.Message.Find(testMessage.Id);
            dbMessage.Title.Should().Be("New Title!");
        }
        
        [Fact]
        public void Put_ShouldReturnBadRequest()
        {
            var controller = new MessagesController(context, logger.Object, mapper);
            var testMessage = TestData.CreateMessage("1", "test description", 0);
            context.Message.Add(testMessage);
            context.SaveChanges();

            var result = controller.Put(testMessage.Id, messageVM: null) as ObjectResult ;

            result.Should().BeOfType<BadRequestObjectResult>();
            result.StatusCode.Should().Be(400);
        }

        [Fact]
        public void Put_ShouldReturnNotFound()
        {
            var controller = new MessagesController(context, logger.Object, mapper);
            var testMessage = TestData.CreateMessage("1", "test description", 0);

            var result = controller.Put(testMessage.Id, messageVM: null) as ObjectResult;

            result.Should().BeOfType<NotFoundObjectResult>();
            result.StatusCode.Should().Be(404);
        }
    }
}
