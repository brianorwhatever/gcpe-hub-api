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
    public class MessagesControllerTests : IDisposable
    {
        private Mock<ILogger<MessagesController>> logger;
        private HubDbContext context;
        private IMapper mapper;
        private DbContextOptions<HubDbContext> options;
        private MessagesController controller;


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
            controller = new MessagesController(context, logger.Object, mapper);
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
                context.Message.Add(TestData.CreateDbMessage(i.ToString(), "test description", 0, Guid.NewGuid(), true, false));
            }
            context.SaveChanges();

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
                context.Message.Add(TestData.CreateDbMessage($"published-{i.ToString()}", "test description", 0, Guid.NewGuid(), true, false));
            }
            for (var i = 0; i < unpublishedCount; i++)
            {
                context.Message.Add(TestData.CreateDbMessage($"unpublished-{i.ToString()}", "test description", 0, Guid.NewGuid(), false, false));
            }
            context.SaveChanges();

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
            var badController = new MessagesController(mockContext.Object, logger.Object, mapper);

            var result = badController.GetAllMessages() as ObjectResult;

            result.StatusCode.Should().Be(400);
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public void GetAll_ShouldntReturnDeletedMessages()
        {
            for (var i = 0; i < 3; i++)
            {
                var message = TestData.CreateDbMessage(i.ToString(), "test description", 0, Guid.NewGuid(), true, false);
                message.IsActive = false;
                context.Message.Add(message);
            }
            context.SaveChanges();

            var result = controller.GetAllMessages() as ObjectResult;

            result.Should().BeOfType<OkObjectResult>();
            result.Should().NotBeNull();
            var models = result.Value as ICollection<Models.Message>;
            models.Should().NotBeNull();
            models.Count().Should().Be(0);
        }

        [Fact]
        public void Post_ShouldReturnSuccess()
        {
            var testDbMessage = TestData.CreateDbMessage("2018MESSAGE-1", "test description", 0);

            var result = controller.AddMessage(mapper.Map<Models.Message>(testDbMessage)) as ObjectResult;

            result.StatusCode.Should().Be(201);
            result.Should().BeOfType<CreatedAtRouteResult>();
            var model = result.Value as Models.Message;
            model.Title.Should().Be("2018MESSAGE-1");
        }

        [Fact]
        public void Post_ShouldSortNewlyPublishedAtTop()
        {
            var testDbMessage = TestData.CreateDbMessage("Top message", "test description", 100, Guid.Empty, true, false);
            for (var i = 0; i < 5; i++)
            {
                context.Message.Add(TestData.CreateDbMessage(i.ToString(), "test description", i, Guid.NewGuid(), true, false));
            }
            context.SaveChanges();

            var testMessage = mapper.Map<Models.Message>(testDbMessage);
            var result = controller.AddMessage(testMessage) as ObjectResult;

            result.Should().BeOfType<CreatedAtRouteResult>();
            result.StatusCode.Should().Be(201);
            var messages = context.Message.Where(m => m.IsPublished).OrderBy(m => m.SortOrder).ToList();
            messages[0].Title.Should().Be("Top message");
            messages[1].Title.Should().Be("0");
            messages[5].Title.Should().Be("4");
        }

        [Fact]
        public void Post_ShouldReturnBadRequest()
        {
            var testDbMessage = TestData.CreateDbMessage("1", "test description", 0, Guid.NewGuid());

            var result = controller.AddMessage(mapper.Map<Models.Message>(testDbMessage)) as ObjectResult;

            result.Should().BeOfType<BadRequestObjectResult>();
            result.StatusCode.Should().Be(400);
        }

        [Fact]
        public void Post_ShouldAllowFirstMessageToBeHighlighted()
        {
            var secondTestDbMessage = TestData.CreateDbMessage("Test Message Title 2", "Test description of the test message that will be highlighted", 0, Guid.Empty, true, true);
            var newHighlightedMessage = mapper.Map<Models.Message>(secondTestDbMessage);

            var result = controller.AddMessage(newHighlightedMessage) as ObjectResult;

            result.Should().BeOfType<CreatedAtRouteResult>();
            result.StatusCode.Should().Be(201);
            var model = result.Value as Models.Message;
            model.IsPublished.Should().BeTrue();
            model.IsHighlighted.Should().BeTrue();
        }

        [Fact]
        public void Post_ShouldOnlyAllowOnePublishedHighlightedMessage()
        {
            var testDbMessage = TestData.CreateDbMessage("Test Message Title", "Test description of the test message that won't be highlighted", 0, Guid.NewGuid(), true, true);
            context.Message.Add(testDbMessage);
            context.SaveChanges();
            var testDbMessageToHighlight = TestData.CreateDbMessage("Test Message Title 2", "Test description of the test message that will be highlighted", 0, Guid.Empty, true, true);
            var newHighlightedMessage = mapper.Map<Models.Message>(testDbMessageToHighlight);

            var result = controller.AddMessage(newHighlightedMessage) as ObjectResult;

            result.Should().BeOfType<CreatedAtRouteResult>();
            result.StatusCode.Should().Be(201);
            var model = result.Value as Models.Message;
            model.IsPublished.Should().BeTrue();
            model.IsHighlighted.Should().BeTrue();
            testDbMessage = context.Message.Find(testDbMessage.Id);
            testDbMessage.IsHighlighted.Should().BeFalse();
            testDbMessage.IsPublished.Should().BeTrue();
        }

        [Fact]
        public void Get_ShouldReturnSuccess()
        {
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
            var testDbMessage = TestData.CreateDbMessage("1", "test description", 0);
            context.Message.Add(testDbMessage);
            context.SaveChanges();
            var badController = new MessagesController(mockContext.Object, logger.Object, mapper);

            var result = badController.GetMessage(testDbMessage.Id) as ObjectResult;

            result.Should().BeOfType<BadRequestObjectResult>();
            result.StatusCode.Should().Be(400);
        }

        [Fact]
        public void Get_ShouldReturnNotFound()
        {
            var result = controller.GetMessage(Guid.NewGuid()) as ObjectResult;

            result.Should().BeOfType<NotFoundObjectResult>();
            result.StatusCode.Should().Be(404);
        }

        [Fact]
        public void Get_ShouldntReturnDeletedMessage()
        {
            var testDbMessage = TestData.CreateDbMessage("2018MESSAGE-1", "test description", 0);
            testDbMessage.IsActive = false;
            context.Message.Add(testDbMessage);
            context.SaveChanges();

            var result = controller.GetMessage(testDbMessage.Id) as ObjectResult;

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
            var testDbMessage = TestData.CreateDbMessage("1", "test description", 0);

            var result = controller.UpdateMessage(testDbMessage.Id, message: null) as ObjectResult;

            result.Should().BeOfType<NotFoundObjectResult>();
            result.StatusCode.Should().Be(404);
        }

        [Fact]
        public void Put_ShouldntUpdateDeletedMessage()
        {
            var testDbMessage = TestData.CreateDbMessage("1", "test description", 0);
            testDbMessage.IsActive = false;
            context.Message.Add(testDbMessage);
            context.SaveChanges();

            var result = controller.UpdateMessage(testDbMessage.Id, mapper.Map<Models.Message>(testDbMessage)) as ObjectResult;

            result.Should().BeOfType<NotFoundObjectResult>();
            result.StatusCode.Should().Be(404);
        }

        [Fact]
        public void Put_ShouldOnlyAllowOnePublishedHighlightedMessage()
        {
            var testDbMessage = TestData.CreateDbMessage("Test Message Title", "Test description of the test message that will be highlighted", 0, Guid.NewGuid(), true, true);
            context.Message.Add(testDbMessage);
            var testDbMessageToHighlight = TestData.CreateDbMessage("Test Message Title 2", "updated description", 0, Guid.NewGuid(), false, false);
            context.Message.Add(testDbMessageToHighlight);
            context.SaveChanges();
            var testUpdatedMessage = mapper.Map<Models.Message>(testDbMessageToHighlight);
            testUpdatedMessage.IsHighlighted = true;
            testUpdatedMessage.IsPublished = true;

            var result = controller.UpdateMessage(testDbMessageToHighlight.Id, testUpdatedMessage) as ObjectResult;

            result.Should().BeOfType<OkObjectResult>();
            result.StatusCode.Should().Be(200);
            testDbMessage = context.Message.Find(testDbMessage.Id);
            testDbMessage.IsHighlighted.Should().BeFalse();
            testDbMessage.IsPublished.Should().BeTrue();
            testDbMessageToHighlight = context.Message.Find(testDbMessageToHighlight.Id);
            testDbMessageToHighlight.IsHighlighted.Should().BeTrue();
            testDbMessageToHighlight.IsPublished.Should().BeTrue();
        }

        [Fact]
        public void Put_ShouldSortNewlyPublishedAtTop()
        {
            var testDbMessage = TestData.CreateDbMessage("Top message", "test description", 100, Guid.NewGuid(), false);
            for (var i = 0; i < 5; i++)
            {
                context.Message.Add(TestData.CreateDbMessage(i.ToString(), "test description", i, Guid.NewGuid(), true));
            }
            context.Message.Add(testDbMessage);
            context.SaveChanges();
            var testMessage = mapper.Map<Models.Message>(testDbMessage);
            testMessage.IsPublished = true;

            var result = controller.UpdateMessage(testMessage.Id, testMessage) as ObjectResult;

            result.Should().BeOfType<OkObjectResult>();
            result.StatusCode.Should().Be(200);
            var messages = context.Message.Where(m => m.IsPublished).OrderBy(m => m.SortOrder).ToList();
            messages[0].Title.Should().Be("Top message");
            messages[1].Title.Should().Be("0");
            messages[5].Title.Should().Be("4");
        }

        [Fact]
        public void Delete_ShouldReturnSuccess()
        {
            var testDbMessage = TestData.CreateDbMessage("message title", "Message description", 0);
            context.Message.Add(testDbMessage);
            context.SaveChanges();

            var result = controller.DeleteMessage(testDbMessage.Id) as StatusCodeResult;

            result.Should().BeOfType<NoContentResult>();
            result.StatusCode.Should().Be(204);
        }

        [Fact]
        public void Delete_ShouldReturnBadRequest()
        {
            var mockContext = new Mock<HubDbContext>(options);
            mockContext.Setup(m => m.Message).Throws(new Exception());
            var badController = new MessagesController(mockContext.Object, logger.Object, mapper);

            var result = badController.DeleteMessage(Guid.NewGuid()) as ObjectResult;

            result.Should().BeOfType<BadRequestObjectResult>();
            result.StatusCode.Should().Be(400);
        }

        [Fact]
        public void Delete_ShouldReturnNotFound()
        {
            var result = controller.DeleteMessage(Guid.NewGuid()) as ObjectResult;

            result.Should().BeOfType<NotFoundObjectResult>();
            result.StatusCode.Should().Be(404);
        }
    }
}
