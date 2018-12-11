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
    public class MessagesControllerTests : IDisposable
    {
        private Mock<ILogger<MessagesController>> logger;
        private HubDbContext context;
        private IMapper mapper;
        private DbContextOptions<HubDbContext> options;

        public MessagesControllerTests()
        {
            options = new DbContextOptionsBuilder<HubDbContext>()
                      .UseInMemoryDatabase(Guid.NewGuid().ToString())
                      .Options;
            context = new HubDbContext(this.options);

            logger = new Mock<ILogger<MessagesController>>();

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
        public void GetAll_ShouldReturnSuccess(int messageCount)
        {
            for (var i = 0; i < messageCount; i++)
            {
                var dbMessage = TestData.CreateDbMessage(i.ToString(), "test description", 0, true, false);
                dbMessage.Id = Guid.NewGuid();
                context.Message.Add(dbMessage);
            }
            context.SaveChanges();
            var controller = new MessagesController(context, logger.Object, mapper);

            var result = controller.GetAllMessages() as ObjectResult;

            result.Should().BeOfType<OkObjectResult>();
            result.Should().NotBeNull();
            var models = result.Value as ICollection<Models.Message>;
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
                var testDbMessage = TestData.CreateDbMessage($"published-{i.ToString()}", "test description", 0, true, false);
                testDbMessage.Id = Guid.NewGuid();
                context.Message.Add(testDbMessage);
            }
            for (var i = 0; i < unpublishedCount; i++)
            {
                var testMessage = TestData.CreateDbMessage($"unpublished-{i.ToString()}", "test description", 0, false, false);
                testMessage.Id = Guid.NewGuid();
                context.Message.Add(testMessage);
            }
            context.SaveChanges();
            var controller = new MessagesController(context, logger.Object, mapper);

            var result = controller.GetAllMessages(isPublished) as ObjectResult;

            result.Should().BeOfType<OkObjectResult>();
            var models = result.Value as ICollection<Models.Message>;
            models.Count().Should().Be(isPublished ? publishedCount : unpublishedCount);
        }

        [Fact]
        public void GetAll_ShouldReturnBadRequest()
        {
            var mockContext = new Mock<HubDbContext>(options);
            mockContext.Setup(m => m.Message).Throws(new Exception());
            var controller = new MessagesController(mockContext.Object, logger.Object, mapper);

            var result = controller.GetAllMessages() as ObjectResult;

            result.StatusCode.Should().Be(400);
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public void Post_ShouldReturnSuccess()
        {
            var controller = new MessagesController(context, logger.Object, mapper);
            var testDbMessage = TestData.CreateDbMessage("2018MESSAGE-1", "test description", 0);

            var result = controller.AddMessage(mapper.Map<Models.Message>(testDbMessage)) as ObjectResult;

            result.StatusCode.Should().Be(201);
            result.Should().BeOfType<CreatedAtRouteResult>();
            var model = result.Value as Models.Message;
            model.Title.Should().Be("2018MESSAGE-1");
        }

        [Fact]
        public void Post_ShouldReturnBadRequest()
        {
            var controller = new MessagesController(context, logger.Object, mapper);
            var testDbMessage = TestData.CreateDbMessage("1", "test description", 0);
            testDbMessage.Id = Guid.NewGuid();

            var result = controller.AddMessage(mapper.Map<Models.Message>(testDbMessage)) as ObjectResult;

            result.Should().BeOfType<BadRequestObjectResult>();
            result.StatusCode.Should().Be(400);
        }

        [Fact]
        public void Get_ShouldReturnSuccess()
        {
            var controller = new MessagesController(context, logger.Object, mapper);
            var testDbMessage = TestData.CreateDbMessage("2018MESSAGE-1", "test description", 0);
            context.Message.Add(testDbMessage);
            context.SaveChanges();

            var result = controller.GetMessage(testDbMessage.Id) as ObjectResult;

            result.Should().BeOfType<OkObjectResult>();
            result.StatusCode.Should().Be(200);
            var model = result.Value as Models.Message;
            model.Title.Should().Be("2018MESSAGE-1");
        }

        [Fact]
        public void Get_ShouldReturnFail()
        {
            var mockContext = new Mock<HubDbContext>(options);
            mockContext.Setup(m => m.Message).Throws(new Exception());
            var controller = new MessagesController(mockContext.Object, logger.Object, mapper);
            var testDbMessage = TestData.CreateDbMessage("1", "test description", 0);
            context.Message.Add(testDbMessage);
            context.SaveChanges();

            var result = controller.GetMessage(testDbMessage.Id) as ObjectResult;

            result.Should().BeOfType<BadRequestObjectResult>();
            result.StatusCode.Should().Be(400);
        }

        [Fact]
        public void Get_ShouldReturnNotFound()
        {
            var controller = new MessagesController(context, logger.Object, mapper);

            var result = controller.GetMessage(Guid.NewGuid()) as ObjectResult;

            result.Should().BeOfType<NotFoundObjectResult>();
            result.StatusCode.Should().Be(404);
        }
        
        [Fact]
        public void Put_ShouldReturnSuccess()
        {
            var testDbMessage = TestData.CreateDbMessage("1", "test description", 0);

            context.Message.Add(testDbMessage);
            context.SaveChanges();
            var testMessage = mapper.Map<Models.Message>(testDbMessage);
            testMessage.Title = "New Title!";


            var controller = new MessagesController(context, logger.Object, mapper);
            var result = controller.UpdateMessage(testDbMessage.Id, testMessage) as ObjectResult;

            result.Should().BeOfType<OkObjectResult>();
            result.StatusCode.Should().Be(200);
            var model = result.Value as Models.Message;
            model.Title.Should().Be("New Title!");
            var dbMessage = context.Message.Find(testDbMessage.Id);
            dbMessage.Title.Should().Be("New Title!");
        }
        
        [Fact]
        public void Put_ShouldReturnBadRequest()
        {
            var controller = new MessagesController(context, logger.Object, mapper);
            var testDbMessage = TestData.CreateDbMessage("1", "test description", 0);
            context.Message.Add(testDbMessage);
            context.SaveChanges();

            var result = controller.UpdateMessage(testDbMessage.Id, message: null) as ObjectResult ;

            result.Should().BeOfType<BadRequestObjectResult>();
            result.StatusCode.Should().Be(400);
        }

        [Fact]
        public void Put_ShouldReturnNotFound()
        {
            var controller = new MessagesController(context, logger.Object, mapper);
            var testDbMessage = TestData.CreateDbMessage("1", "test description", 0);

            var result = controller.UpdateMessage(testDbMessage.Id, message: null) as ObjectResult;

            result.Should().BeOfType<NotFoundObjectResult>();
            result.StatusCode.Should().Be(404);
        }
    }
}
