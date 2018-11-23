using AutoMapper;
using Gcpe.Hub.API.Models;
using Microsoft.Extensions.Logging;
using Moq;
using AutoMapper;
using System;
using FluentAssertions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gcpe.Hub.API.Controllers;
using Gcpe.Hub.API.ViewModels;
using Gcpe.Hub.API.Data;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Gcpe.Hub.API.Tests.ControllerTests
{
    public class MessagesControllerTests
    {
        private readonly Message _expectedModelReturn;
        private Mock<ILogger<MessagesController>> _logger;
        private Mock<IMapper> _mapper;

        public MessagesControllerTests()
        {
            _expectedModelReturn = TestData.TestMessage;
            _logger = new Mock<ILogger<MessagesController>>();
            _mapper = CreateMapper();
        }

        private Mock<IRepository> CreateDataStore()
        {
            var dataStore = new Mock<IRepository>();
            dataStore.Setup(r => r.GetAllMessages()).Returns(TestData.TestMessages);
            return dataStore;
        }

        private Mock<IMapper> CreateMapper()
        {
            var mapper = new Mock<IMapper>();
            mapper.Setup(m => m.Map<Message, MessageViewModel>(It.IsAny<Message>()))
                .Returns((Message entity) => new MessageViewModel
                {
                    Id = entity.Id,
                    Title = entity.Title,
                    Description = entity.Description,
                    Timestamp = entity.Timestamp,
                    SortOrder = entity.SortOrder,
                    IsHighlighted = entity.IsHighlighted,
                    IsPublished = entity.IsPublished
                });

            mapper.Setup(m => m.Map<MessageViewModel, Message>(It.IsAny<MessageViewModel>()))
                .Returns((MessageViewModel data) => new Message
                {
                    Id = data.Id,
                    Title = data.Title,
                    Description = data.Description,
                    Timestamp = data.Timestamp,
                    SortOrder = data.SortOrder,
                    IsHighlighted = data.IsHighlighted,
                    IsPublished = data.IsPublished
                });

            return mapper;
        }

        [Fact]
        public void GetAllMessages_ShouldReturnSuccess()
        {
            var mockRepository = CreateDataStore();
            var expectedMessagesCount = TestData.TestMessages.Count();
            var controller = new MessagesController(mockRepository.Object, _logger.Object, _mapper.Object);

            var results = controller.GetResultsPage();
            var numberOfMessages = results.Count();

            numberOfMessages.Should().Be(expectedMessagesCount);
        }

        [Fact]
        public void Create_ShouldReturnSuccess()
        {
            var mockRepository = CreateDataStore();
            var controller = new MessagesController(mockRepository.Object, _logger.Object, _mapper.Object);

            var result = controller.CreateMessage(TestData.TestMessage);

            result.Should().BeOfType<OkResult>()
                .Which.StatusCode.Should().Be((int)HttpStatusCode.OK);
        }
    }
}
