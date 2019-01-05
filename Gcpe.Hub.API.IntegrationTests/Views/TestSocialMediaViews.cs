using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json;
using Xunit;

namespace Gcpe.Hub.API.IntegrationTests.Views
{
    public class TestSocialMediaViews: BaseWebApiTest
    {
        public TestSocialMediaViews(CustomWebApplicationFactory<Startup> factory) : base(factory) { }

        private async Task<Models.SocialMediaPost> _PostSocialMediaPost(int sortOrder = 0, string url = "http://facebook.com/post/123")
        {
            StringContent testPost = TestData.CreateSocialMediaPost(url, sortOrder);
            var createResponse = await Client.PostAsync("/api/socialmediaposts", testPost);
            var createBody = await createResponse.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Models.SocialMediaPost>(createBody);
        }

        [Fact]
        public async Task List_EndpointShouldReturnSuccessAndCorrectPostsSorted()
        {
            for (var i = 0; i < 5; i++)
            {
                await _PostSocialMediaPost(5 - i);
            }

            var response = await Client.GetAsync("/api/socialmediaposts");
            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadAsStringAsync();
            var models = JsonConvert.DeserializeObject<Models.SocialMediaPost[]>(body);

            models.Should().NotBeEmpty();
            models.Should().HaveCount(5);

            for (int i = 0; i < models.Length - 1; i++)
            {
                models[i].SortOrder.Should().BeLessThan(models[i + 1].SortOrder);
            }
        }

        [Fact]
        public async Task List_EndpointShouldReturnSuccessAndHandleIfModifiedSince()
        {
            await _PostSocialMediaPost();

            var response = await Client.GetAsync("/api/socialmediaposts");
            response.EnsureSuccessStatusCode();

            DateTimeOffset? lastModified = response.Content.Headers.LastModified;
            Client.DefaultRequestHeaders.IfModifiedSince = lastModified;
            response = await Client.GetAsync("/api/socialmediaposts");
            response.StatusCode.Should().Be(304);
        }

        [Fact]
        public async Task Create_EndpointShouldReturnSuccessAndCorrectPost()
        {
            var url = "http://facebook.com/post/2345";
            var createdPost = await _PostSocialMediaPost(0, url);

            createdPost.Url.Should().Be(url);
        }

        [Fact]
        public async Task Create_EndpointShouldRequireUrl()
        {
            var invalidPost = new Models.SocialMediaPost
            {
                SortOrder = 0,
            };
            var stringContent = new StringContent(JsonConvert.SerializeObject(invalidPost), Encoding.UTF8, "application/json");

            var response = await Client.PostAsync("/api/socialmediaposts", stringContent);
            response.IsSuccessStatusCode.Should().BeFalse();
        }

        [Fact]
        public async Task Get_EndpointShouldReturnSuccessAndCorrectPost()
        {
            Guid id = (await _PostSocialMediaPost()).Id;

            var response = await Client.GetAsync($"/api/socialmediaposts/{id}");
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            var postResult = JsonConvert.DeserializeObject<Models.SocialMediaPost>(body);

            postResult.Url.Should().Be(postResult.Url);
            postResult.Id.Should().Be(id.ToString());
        }

        [Fact]
        public async Task Get_EndpointShouldReturnNotFound()
        {
            var response = await Client.GetAsync($"/api/socialmediaposts/{Guid.NewGuid()}");

            response.IsSuccessStatusCode.Should().BeFalse();
        }

        [Fact]
        public async Task Put_EndpointShouldReturnSuccessAndCorrectMessage()
        {
            int sortOrder = 10;
            string url = "http://twitter.com/post/123";
            Guid id = (await _PostSocialMediaPost()).Id;

            var newPost = TestData.CreateSocialMediaPost(url, sortOrder);
            var response = await Client.PutAsync($"/api/socialmediaposts/{id}", newPost);
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            var postResult = JsonConvert.DeserializeObject<Models.SocialMediaPost>(body);

            postResult.Url.Should().Be(url);
            postResult.SortOrder.Should().Be(sortOrder);
            postResult.Id.Should().Be(id.ToString());
        }

        [Fact]
        public async Task Put_EndpointShouldRequireUrl()
        {
            Guid id = (await _PostSocialMediaPost()).Id;

            var content = new StringContent(JsonConvert.SerializeObject(new { }), Encoding.UTF8, "application/json");
            var response = await Client.PutAsync($"/api/socialmediaposts/{id}", content);
            response.IsSuccessStatusCode.Should().BeFalse();
        }

        [Fact]
        public async Task Delete_EndpointShouldDelete()
        {
            Guid id = (await _PostSocialMediaPost()).Id;

            var response = await Client.DeleteAsync($"/api/socialmediaposts/{id}");
            response.EnsureSuccessStatusCode();

            var getResponse = await Client.GetAsync($"/api/socialmediaposts/{id}");
            getResponse.IsSuccessStatusCode.Should().BeFalse();
            getResponse.StatusCode.Should().Be(404);

            var getAllResponse = await Client.GetAsync($"/api/socialmediaposts");
            getAllResponse.EnsureSuccessStatusCode();
            var body = await getAllResponse.Content.ReadAsStringAsync();
            var models = JsonConvert.DeserializeObject<Models.SocialMediaPost[]>(body);

            foreach (Models.SocialMediaPost post in models)
            {
                post.Id.Should().NotBe(id);
            }
        }
    }
}
