using System;
using System.Linq;
using AutoMapper;
using FluentAssertions;
using Gcpe.Hub.API.Controllers;
using Gcpe.Hub.API.Data;
using Gcpe.Hub.API.Helpers;
using Gcpe.Hub.Data.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Gcpe.Hub.API.Tests.ControllerTests
{
    public class NewsReleasesControllerTests
    {
        private readonly NewsRelease _expectedModelReturn;
        private Mock<ILogger<NewsReleasesController>> _logger;
        private IMapper _mapper;

        public NewsReleasesControllerTests()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _expectedModelReturn = TestData.TestNewsRelease;
            _logger = new Mock<ILogger<NewsReleasesController>>();
            var mockMapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new MappingProfile());
            });
            _mapper = mockMapper.CreateMapper();
        }

        private Mock<IRepository> CreateDataStore()
        {
            var dataStore = new Mock<IRepository>();
            dataStore.Setup(r => r.GetAllReleases()).Returns(TestData.TestNewsReleases);
            dataStore.Setup(r => r.GetReleaseByKey("0")).Returns(() => _expectedModelReturn);
            dataStore.Setup(r => r.GetReleaseByKey(It.IsNotIn("0"))).Returns(() => null);
            return dataStore;
        }

        [Fact]
        public void GetById_ShouldReturnSuccess()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var mockRepository = CreateDataStore();
            var controller = new NewsReleasesController(mockRepository.Object, _logger.Object, _mapper);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = controller.GetById("0");

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().BeOfType(typeof(OkObjectResult), "because the read operation should go smoothly");

            var actualValue = ((result as OkObjectResult).Value as Models.NewsRelease);
            actualValue.Key.Should().Be(_expectedModelReturn.Key);
            actualValue.PublishDateTime.Should().Be(_expectedModelReturn.PublishDateTime);
            actualValue.Keywords.Should().Be(_expectedModelReturn.Keywords);
        }

        [Fact]
        public void GetById_ShouldReturnNotFound_WhenGivenInvalidId()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var mockRepository = CreateDataStore();
            var controller = new NewsReleasesController(mockRepository.Object, _logger.Object, _mapper);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = controller.GetById("-1");  // does not exist...

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().BeOfType(typeof(NotFoundResult), "because an invalid Id should not yield a result");
        }

        [Fact]
        public void GetById_ShouldReturnBadRequest_WhenDataSourceIsUnavailable()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var validId = "0";
            var mockRepository = CreateDataStore();
            mockRepository.Setup(r => r.GetReleaseByKey(It.IsAny<string>())).Throws<InvalidOperationException>();
            var controller = new NewsReleasesController(mockRepository.Object, _logger.Object, _mapper);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = controller.GetById(validId) as BadRequestObjectResult;

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().BeOfType(typeof(BadRequestObjectResult), "because the read operation should require a valid data source");
            result.StatusCode.Should().Be(400, "because HTTP Status 400 should be returned to signal a Bad Request");
        }

        [Fact]
        public void GetAllNewsReleases_ShouldReturnSuccess()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var paginationParams = new NewsReleaseParams();
            var expectedReleasesPerPage = paginationParams.PageSize;
            var mockRepository = CreateDataStore();
            var controller = new NewsReleasesController(mockRepository.Object, _logger.Object, _mapper);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var results = controller.GetResultsPage(paginationParams);
            var actualNumberOfReleases = results.Count();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            actualNumberOfReleases.Should().Be(expectedReleasesPerPage);
        }

        [Fact]
        public void GetAllNewsReleases_ShouldReturnBadRequest_WhenDataSourceIsUnavailable()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var mockRepository = CreateDataStore();
            mockRepository.Setup(r => r.GetAllReleases()).Throws<InvalidOperationException>();
            var controller = new NewsReleasesController(mockRepository.Object, _logger.Object, _mapper);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = controller.GetAll(newsReleaseParams: new NewsReleaseParams()) as BadRequestObjectResult;

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().BeOfType(typeof(BadRequestObjectResult), "because the read operation should require a valid data source");
            result.StatusCode.Should().Be(400, "because HTTP Status 400 should be returned to signal a Bad Request");
        }

        [Fact]
        public void Post_ShouldCreateNewEntityAndReturnSuccess()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var releaseToCreate = new Models.NewsRelease
            {
                Key = TestData.TestNewsRelease.Key,
                PublishDateTime = DateTime.Now
            };
            var mockRepository = CreateDataStore();
            mockRepository.Setup(r => r.AddEntity(It.IsAny<NewsRelease>())).Verifiable();
            var controller = new NewsReleasesController(mockRepository.Object, _logger.Object, _mapper);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = controller.Post(releaseToCreate) as StatusCodeResult;

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().BeOfType(typeof(StatusCodeResult), "because the create operation should go smoothly");
            result.StatusCode.Should().Be(201, "because HTTP Status 201 should be returned upon creation of new entity");
            // this will throw if the System-Under-Test (SUT) i.e. the controller didn't call repository.AddEntity(...)
            //mockRepository.Verify();
        }

        [Fact]
        public void Post_ShouldReturnBadRequest_WhenGivenInvalidModel()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var mockRepository = CreateDataStore();
            var controller = new NewsReleasesController(mockRepository.Object, _logger.Object, _mapper);
            controller.ModelState.AddModelError("error", "some validation error");

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = controller.Post(release: null) as BadRequestObjectResult;

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
            var mockRepository = CreateDataStore();
            mockRepository.Setup(r => r.Update("0", It.IsAny<NewsRelease>())).Returns(_expectedModelReturn);
            var controller = new NewsReleasesController(mockRepository.Object, _logger.Object, _mapper);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = controller.Put("0", _mapper.Map<Models.NewsRelease>(_expectedModelReturn));

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().BeOfType(typeof(OkObjectResult), "because the update operation should go smoothly");
        }

        [Fact]
        public void Put_ShouldReturnNotFound_WhenGivenInvalidId()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var mockRepository = CreateDataStore();
            var controller = new NewsReleasesController(mockRepository.Object, _logger.Object, _mapper);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = controller.Put("-1", _mapper.Map<Models.NewsRelease>(_expectedModelReturn));  // does not exist...

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().BeOfType(typeof(NotFoundObjectResult), "because a valid Id is required to update an entity");
        }

        [Fact]
        public void Put_ShouldReturnBadRequest_WhenDataSourceIsUnavailable()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var mockRepository = CreateDataStore();
            mockRepository.Setup(r => r.GetReleaseByKey(It.IsAny<string>())).Throws<InvalidOperationException>();
            var controller = new NewsReleasesController(mockRepository.Object, _logger.Object, _mapper);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = controller.Put("0", _mapper.Map<Models.NewsRelease>(_expectedModelReturn)) as BadRequestObjectResult;

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().BeOfType(typeof(BadRequestObjectResult), "because the update operation should require a valid data source");
            result.StatusCode.Should().Be(400, "because HTTP Status 400 should be returned to signal a Bad Request");
        }

        [Fact]
        public void Delete_ShouldDeleteEntityAndReturnSuccess()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var mockRepository = CreateDataStore();
            mockRepository.Setup(r => r.Delete(It.IsAny<NewsRelease>())).Verifiable();
            var controller = new NewsReleasesController(mockRepository.Object, _logger.Object, _mapper);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = controller.Delete("0");

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().BeOfType(typeof(OkResult), "because the delete operation should go smoothly");
            // this will throw if the System-Under-Test (SUT) i.e. the controller didn't call repository.Delete(...)
            mockRepository.Verify();
        }

        [Fact]
        public void Delete_ShouldReturnNotFound_WhenGivenInvalidId()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var mockRepository = CreateDataStore();
            var controller = new NewsReleasesController(mockRepository.Object, _logger.Object, _mapper);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = controller.Delete("-1");  // does not exist...

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().BeOfType(typeof(NotFoundObjectResult), "because a valid Id is required to delete an entity");
        }

        [Fact]
        public void Delete_ShouldReturnBadRequest_WhenDataSourceIsUnavailable()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var mockRepository = CreateDataStore();
            mockRepository.Setup(r => r.GetReleaseByKey(It.IsAny<string>())).Throws<InvalidOperationException>();
            var controller = new NewsReleasesController(mockRepository.Object, _logger.Object, _mapper);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = controller.Delete("0") as BadRequestObjectResult;

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().BeOfType(typeof(BadRequestObjectResult), "because the delete operation should require a valid data source");
            result.StatusCode.Should().Be(400, "because HTTP Status 400 should be returned to signal a Bad Request");
        }
    }
}
