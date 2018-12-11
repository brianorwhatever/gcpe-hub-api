using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using FluentAssertions;
using Gcpe.Hub.API.Controllers;
using Gcpe.Hub.API.Data;
using Gcpe.Hub.Data.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Gcpe.Hub.API.Tests.ControllerTests
{
    public class NewsReleaseLogsControllerTests
    {
        private readonly NewsRelease _expectedModelReturn;
        private Mock<ILogger<NewsReleaseLogsController>> _logger;
        private IMapper _mapper;

        public NewsReleaseLogsControllerTests()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _expectedModelReturn = TestData.TestNewsRelease;
            _logger = new Mock<ILogger<NewsReleaseLogsController>>();
            var mockMapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new MappingProfile());
            });
            _mapper = mockMapper.CreateMapper();
        }

        private Mock<IRepository> CreateDataStore()
        {
            var dataStore = new Mock<IRepository>();
            dataStore.Setup(r => r.GetReleaseByKey("0")).Returns(() => _expectedModelReturn);
            dataStore.Setup(r => r.GetReleaseByKey(It.IsNotIn("0"))).Returns(() => null);
            return dataStore;
        }

        [Fact]
        public void GetAll_ShouldReturnSuccess()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var expectedCount = _expectedModelReturn.NewsReleaseLog.Count;
            var expectedLogEntry = _expectedModelReturn.NewsReleaseLog.FirstOrDefault();
            var mockRepository = CreateDataStore();
            var controller = new NewsReleaseLogsController(mockRepository.Object, _logger.Object, _mapper);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = controller.GetAll("0");

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().BeOfType(typeof(OkObjectResult), "because the read operation should go smoothly");
            var actual = ((result as OkObjectResult).Value as IEnumerable<Models.NewsReleaseLog>);
            var actualLogEntry = actual.FirstOrDefault();

            actual.Count().Should().Be(expectedCount);
            actualLogEntry.ReleaseKey.Should().Be(expectedLogEntry.Release.Key);
            actualLogEntry.Description.Should().Be(expectedLogEntry.Description);
        }

        [Fact]
        public void GetAll_ShouldReturnNotFound_WhenGivenInvalidReleaseId()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var mockRepository = CreateDataStore();
            var controller = new NewsReleaseLogsController(mockRepository.Object, _logger.Object, _mapper);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = controller.GetAll("-1");  // does not exist...

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().BeOfType(typeof(NotFoundResult), "because an invalid Id should not yield a result");
        }

        [Fact]
        public void Get_ShouldReturnSuccess()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var mockRepository = CreateDataStore();
            var controller = new NewsReleaseLogsController(mockRepository.Object, _logger.Object, _mapper);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = controller.Get("0", 1);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().BeOfType(typeof(OkObjectResult), "because the read operation should go smoothly");
        }

        [Theory]
        [InlineData("0", -1, "because an invalid LogId should not yield a result")]
        [InlineData("-1", 1, "because an invalid ReleaseId should not yield a result")]
        public void Get_ShouldReturnNotFound_WhenGivenInvalidParameters(string releaseId, int logId, string because = null)
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var mockRepository = CreateDataStore();
            var controller = new NewsReleaseLogsController(mockRepository.Object, _logger.Object, _mapper);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = controller.Get(releaseId, logId);  // does not exist...

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().BeOfType(typeof(NotFoundResult), because);
        }
    }
}
