using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Gcpe.Hub.API.ViewModels;
using Newtonsoft.Json;
using Xunit;

namespace Gcpe.Hub.API.IntegrationTests.Views
{
    public class TestMessagesViews : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private readonly CustomWebApplicationFactory<Startup> _factory;
        public readonly HttpClient _client;
        public StringContent testMessage = TestData.CreateSerializedMessage("Lorem Title", "Lorem description", 0, true, false);

        public TestMessagesViews(CustomWebApplicationFactory<Startup> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        private async Task<Guid> _PostMessage()
        {
            var createResponse = await _client.PostAsync("/api/messages", testMessage);
            var createBody = await createResponse.Content.ReadAsStringAsync();
            var createdPost = JsonConvert.DeserializeObject<MessageViewModel>(createBody);
            return createdPost.Id;
        }

        [Fact]
        public async Task List_EndpointReturnSuccessAndCorrectMessagesSorted()
        {
            for (var i = 0; i < 5; i++)
            {
                int sortOrder = 5 - i;
                var newMessage = TestData.CreateSerializedMessage("Sorted Test Message", "test description", sortOrder, true, false);

                var createResponse = await _client.PostAsync("/api/messages", newMessage);
                createResponse.EnsureSuccessStatusCode();
            }

            var response = await _client.GetAsync("/api/messages");
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            var models = JsonConvert.DeserializeObject<MessageViewModel[]>(body).Where(m => m.Title == "Sorted Test Message");

            for (int i = 0; i < models.Count() - 1; i++)
            {
                models.ElementAt(i).SortOrder.Should().BeLessThan(models.ElementAt(i + 1).SortOrder);
            }
        }

        [Fact]
        public async Task Create_EndpointReturnSuccessAndCorrectMessage()
        {
            var newMessage = TestData.CreateSerializedMessage("Test title!", "test description!", 0, true, false);
            var createResponse = await _client.PostAsync("/api/messages", newMessage);
            createResponse.EnsureSuccessStatusCode();
            var createBody = await createResponse.Content.ReadAsStringAsync();
            var messageResult = JsonConvert.DeserializeObject<MessageViewModel>(createBody);

            messageResult.Title.Should().Be("Test title!");
            messageResult.Description.Should().Be("test description!");
        }

        [Fact]
        public async Task Create_EndpointRequiresTitle()
        {
            var brokenTestMessage = TestData.CreateSerializedMessage(null, "description", 0, true, false);
            var response = await _client.PostAsync("/api/messages", brokenTestMessage);
            response.IsSuccessStatusCode.Should().BeFalse();
        }

        [Fact]
        public async Task Create_EndpointDoesntRequireDescription()
        {
            var noDescMessage = TestData.CreateSerializedMessage("Title", null, 0, true, false);

            var response = await _client.PostAsync("/api/messages", noDescMessage);
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            var messageResult = JsonConvert.DeserializeObject<MessageViewModel>(body);

            messageResult.Title.Should().Be("Title");
            messageResult.Description.Should().BeNull();
        }

        [Fact]
        public async Task Get_EndpointReturnSuccessAndCorrectMessage()
        {
            Guid id = await _PostMessage();

            var response = await _client.GetAsync($"/api/Messages/{id}");
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            var messageResult = JsonConvert.DeserializeObject<MessageViewModel>(body);

            messageResult.Id.Should().Be(id);
        }

        [Fact]
        public async Task Get_EndpointReturnsNotFound()
        {
            var response = await _client.GetAsync($"/api/messages/{Guid.NewGuid()}");

            response.IsSuccessStatusCode.Should().BeFalse();
        }

        [Fact]
        public async Task Put_EndpointReturnSuccessAndCorrectMessage()
        {
            Guid id = await _PostMessage();
            var newTestMessage = TestData.CreateSerializedMessage("new title", "new description", 10, true, false);

            var response = await _client.PutAsync($"/api/messages/{id}", newTestMessage);
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            var messageResult = JsonConvert.DeserializeObject<MessageViewModel>(body);

            messageResult.Title.Should().Be("new title");
            messageResult.Description.Should().Be("new description");
            messageResult.SortOrder.Should().Be(10);
            messageResult.Id.Should().Be(id);
        }

        [Fact]
        public async Task Put_EndpointReturnSuccessWithDefaultsAndCorrectMessage()
        {
            Guid id = await _PostMessage();
            var content = new StringContent(JsonConvert.SerializeObject(new { Title = "new title" }), Encoding.UTF8, "application/json");

            var response = await _client.PutAsync($"/api/messages/{id}", content);
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            var messageResult = JsonConvert.DeserializeObject<MessageViewModel>(body);

            messageResult.Title.Should().Be("new title");
            messageResult.Description.Should().Be(null);
            messageResult.SortOrder.Should().Be(0);
            messageResult.IsPublished.Should().BeFalse();
            messageResult.IsHighlighted.Should().BeFalse();
            messageResult.Id.Should().Be(id);
        }

        [Fact]
        public async Task Put_EndpointShouldRequireTitle()
        {
            Guid id = await _PostMessage();
            
            var content = new StringContent(JsonConvert.SerializeObject(new { }), Encoding.UTF8, "application/json");
            var response = await _client.PutAsync($"/api/messages/{id}", content);
            response.IsSuccessStatusCode.Should().BeFalse();
        }
    }
}

