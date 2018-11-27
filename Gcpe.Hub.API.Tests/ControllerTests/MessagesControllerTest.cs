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
        private readonly Message expectedModelReturn;
        private Mock<ILogger<MessagesController>> logger;
        private HubDbContext context;
        private IMapper mapper;

        public MessagesControllerTests()
        {

            this.context = GetContext();
            this.logger = new Mock<ILogger<MessagesController>>();
            this.mapper = CreateMapper();
        }

        private HubDbContext GetContext()
        {
            var options = new DbContextOptionsBuilder<HubDbContext>()
                      .UseInMemoryDatabase(Guid.NewGuid().ToString())
                      .Options;
            var context = new HubDbContext(options);
            return context;
        }

        private IMapper CreateMapper()
        {
            var mockMapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new MappingProfile());
            });
            var mapper = mockMapper.CreateMapper();
            return mapper;
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(3)]
        public void GetAll_ShouldReturnSuccess(int messageCount)
        {
            for (var i = 0; i < messageCount; i++)
            {
                context.Message.Add(TestData.TestMessage);
            }
            context.SaveChanges();
            var controller = new MessagesController(context, logger.Object, mapper);

            var result = controller.GetAll();
            var okResult = result as OkObjectResult;

            okResult.Should().BeOfType<OkObjectResult>();
            okResult.Should().NotBeNull();

            var models = okResult.Value as ICollection<Message>;
            models.Should().NotBeNull();
            models.Count().Should().Equals(messageCount);
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

            var result = controller.GetAll() as BadRequestObjectResult;

            result.Should().BeOfType<BadRequestObjectResult>();
            result.StatusCode.Should().Be(400);
        }

        [Fact]
        public void Post_ShouldReturnSuccess()
        {
            var controller = new MessagesController(context, logger.Object, mapper);

            var result = controller.Post(mapper.Map<Message, MessageViewModel>(TestData.TestMessage));
            var createdResult = result as CreatedAtRouteResult;

            createdResult.Should().BeOfType<CreatedAtRouteResult>();
            createdResult.StatusCode.Should().Equals(201);

            var model = createdResult.Value as MessageViewModel;
            model.Title.Should().Equals("2018MESSAGE-1");
        }

        [Fact]
        public void Post_ShouldReturnBadRequest()
        {
            var controller = new MessagesController(context, logger.Object, mapper);
            controller.ModelState.AddModelError("error", "some validation error");
            var testMessage = TestData.TestMessage;

            var result = controller.Post(message: null) as BadRequestObjectResult;

            result.Should().BeOfType<BadRequestObjectResult>();
            result.StatusCode.Should().Be(400);
        }

        [Fact]
        public void Get_ShouldReturnSuccess()
        {
            var controller = new MessagesController(context, logger.Object, mapper);
            var testMessage = TestData.TestMessage;
            context.Message.Add(testMessage);
            context.SaveChanges();

            var result = controller.Get(testMessage.Id.ToString()) as OkObjectResult;

            result.Should().BeOfType<OkObjectResult>();
            result.StatusCode.Should().Be(200);

            var model = result.Value as MessageViewModel;
            model.Title.Should().Equals("2018MESSAGE-1");
        }

        [Fact]
        public void Get_ShouldReturnFail()
        {
            var controller = new MessagesController(context, logger.Object, mapper);
            var testMessage = TestData.TestMessage;
            context.Message.Add(testMessage);
            context.SaveChanges();

            var result = controller.Get(testMessage.Id.ToString()+"test") as BadRequestObjectResult;

            result.Should().BeOfType<BadRequestObjectResult>();
            result.StatusCode.Should().Be(400);
        }

        [Fact]
        public void Get_ShouldReturnNotFound()
        {
            var controller = new MessagesController(context, logger.Object, mapper);

            var result = controller.Get(Guid.NewGuid().ToString()) as NotFoundResult;

            result.Should().BeOfType<NotFoundResult>();
            result.StatusCode.Should().Be(404);
        }
        
        [Fact]
        public void Put_ShouldReturnSuccess()
        {
            var controller = new MessagesController(context, logger.Object, mapper);
            var testMessage = TestData.TestMessage;
            context.Message.Add(testMessage);
            context.SaveChanges();
            testMessage.Title = "New Title!";

            var result = controller.Put(testMessage.Id.ToString(), mapper.Map<Message, MessageViewModel>(testMessage)) as OkObjectResult;

            result.Should().BeOfType<OkObjectResult>();
            result.StatusCode.Should().Be(200);

            var model = result.Value as MessageViewModel;
            model.Title.Should().Equals("New Title!");
            var dbMessage = context.Message.Find(testMessage.Id);
            dbMessage.Title.Should().Equals("New Title!");
        }

        [Fact]
        public void Put_ShouldReturnBadRequest()
        {
            var controller = new MessagesController(context, logger.Object, mapper);
            var testMessage = TestData.TestMessage;
            context.Message.Add(testMessage);
            context.SaveChanges();

            var result = controller.Put(testMessage.Id.ToString(), message: null) as BadRequestObjectResult;

            result.Should().BeOfType<BadRequestObjectResult>();
            result.StatusCode.Should().Be(400);
        }

        [Fact]
        public void Put_ShouldReturnNotFound()
        {
            var controller = new MessagesController(context, logger.Object, mapper);
            var testMessage = TestData.TestMessage;

            var result = controller.Put(testMessage.Id.ToString(), message: null) as NotFoundObjectResult;

            result.Should().BeOfType<NotFoundObjectResult>();
            result.StatusCode.Should().Be(404);
        }
    }
}
