using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Gcpe.Hub.Data.Entity;
using Newtonsoft.Json;
using Xunit;

namespace Gcpe.Hub.API.IntegrationTests
{
    public class PostsPageShould : BaseWebApiTest
    {
        private int expectedEntitiesPerPage = 5;

        public PostsPageShould(CustomWebApplicationFactory<Startup> factory) : base(factory)
        {
        }

        [Fact]
        public async Task ReturnAListOfPosts()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------

            for (var i = 0; i < 5; i++)
            {
                var createResponse = await Client.PostAsync("/api/Posts", TestData.CreatePost(i.ToString()));
                createResponse.EnsureSuccessStatusCode();
            }

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await Client.GetAsync("/api/Posts");
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            var deserializedBody = JsonConvert.DeserializeObject<Models.Post[]>(body);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            deserializedBody.Should().HaveCountGreaterOrEqualTo(expectedEntitiesPerPage);
        }

        [Fact]
        public async Task ReturnNewsReleaseGivenValueOf0()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------

            var createResponse = await Client.PostAsync("/api/Posts", TestData.CreatePost("0"));
            createResponse.EnsureSuccessStatusCode();
            createResponse.Headers.Location.LocalPath.Should().Be("/api/Posts/0");


            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await Client.GetAsync(createResponse.Headers.Location);
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            var deserializedBody = JsonConvert.DeserializeObject<Models.Post>(body);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            deserializedBody.Key.Should().NotBeNullOrEmpty();  // `key` property is REQUIRED
        }

        [Fact]
        public async Task RespondWith404GivenValueOf1()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await Client.GetAsync("/api/Posts/1");

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);  // 404 not found
        }
    }
}
