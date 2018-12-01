using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Gcpe.Hub.API.IntegrationTests.Helpers;
using Gcpe.Hub.API.ViewModels;
using Gcpe.Hub.Data.Entity;
using Newtonsoft.Json;
using Xunit;

namespace Gcpe.Hub.API.IntegrationTests.Views
{
    public class MessagesViewsShould : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private readonly CustomWebApplicationFactory<Startup> _factory;
        public readonly HttpClient _client;
        public MessageViewModel testMessage = MessagesTestData.CreateMessage("Test message title",
            "Test message description", 0);

        public MessagesViewsShould(CustomWebApplicationFactory<Startup> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task List_EndpointReturnSuccessAndCorrectMessages()
        {
            for (var i = 0; i < 5; i++)
            {
                var newMessage = MessagesTestData.CreateMessage("Test message title", "Test message description", 0);
                var stringContent = new StringContent(JsonConvert.SerializeObject(newMessage), Encoding.UTF8, "application/json");

                var createResponse = await _client.PostAsync("/api/messages", stringContent);
                createResponse.EnsureSuccessStatusCode();
            }

            var response = await _client.GetAsync("/api/messages");
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            var deserializedBody = JsonConvert.DeserializeObject<Hub.API.ViewModels.MessageViewModel[]>(body);

            Assert.NotEmpty(deserializedBody);
            deserializedBody.Should().HaveCountGreaterOrEqualTo(5);
        }

        [Fact]
        public async Task Create_EndpointReturnSuccessAndCorrectMessage()
        {
            testMessage.Id = Guid.Empty;
            var stringContent = new StringContent(JsonConvert.SerializeObject(testMessage), Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("/api/messages",  stringContent);
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            var messageResult = JsonConvert.DeserializeObject<Hub.API.ViewModels.MessageViewModel>(body);

            messageResult.Title.Should().Be(testMessage.Title);
            messageResult.Description.Should().Be(testMessage.Description);
        }

        [Fact]
        public async Task Create_EndpointRequiresTitle()
        {
            testMessage.Id = Guid.Empty;
            var brokenTestMessage = testMessage;
            brokenTestMessage.Title = null;
            var stringContent = new StringContent(JsonConvert.SerializeObject(brokenTestMessage), Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("/api/messages", stringContent);
            response.IsSuccessStatusCode.Should().BeFalse();
        }

        [Fact]
        public async Task Create_EndpointDoesntRequireDescription()
        {
            testMessage.Id = Guid.Empty;
            var noDescMessage = testMessage;
            noDescMessage.Description = null;
            var stringContent = new StringContent(JsonConvert.SerializeObject(noDescMessage), Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("/api/messages", stringContent);
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            var messageResult = JsonConvert.DeserializeObject<Hub.API.ViewModels.MessageViewModel>(body);

            messageResult.Title.Should().Be(noDescMessage.Title);
            messageResult.Description.Should().BeNull();
        }

        [Fact]
        public async Task Get_EndpointReturnSuccessAndCorrectMessage()
        {
            testMessage.Id = Guid.Empty;
            var stringContent = new StringContent(JsonConvert.SerializeObject(testMessage), Encoding.UTF8, "application/json");
            var createResponse = await _client.PostAsync("/api/messages", stringContent);
            var createBody = await createResponse.Content.ReadAsStringAsync();
            var createdMessage = JsonConvert.DeserializeObject<Hub.API.ViewModels.MessageViewModel>(createBody);
            var id = createdMessage.Id;

            var response = await _client.GetAsync($"/api/Messages/{id}");
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            var messageResult = JsonConvert.DeserializeObject<Hub.API.ViewModels.MessageViewModel>(body);

            messageResult.Title.Should().Be(testMessage.Title);
            messageResult.Description.Should().Be(testMessage.Description);
            messageResult.Id.Should().Be(id);
        }

        [Fact]
        public async Task Get_EndpointNotFound()
        {
            var response = await _client.GetAsync($"/api/messages/{Guid.NewGuid()}");

            response.IsSuccessStatusCode.Should().BeFalse();
        }

        [Fact]
        public async Task Put_EndpointReturnSuccessAndCorrectMessage()
        {
            var stringContent = new StringContent(JsonConvert.SerializeObject(testMessage), Encoding.UTF8, "application/json");
            var createResponse = await _client.PostAsync("/api/messages", stringContent);
            var createBody = await createResponse.Content.ReadAsStringAsync();
            var createdMessage = JsonConvert.DeserializeObject<Hub.API.ViewModels.MessageViewModel>(createBody);
            var id = createdMessage.Id;

            var newTestMessage = MessagesTestData.CreateMessage("new title", "new description", 10, true, false);
            newTestMessage.Id = Guid.Empty;

            var content = new StringContent(JsonConvert.SerializeObject(newTestMessage), Encoding.UTF8, "application/json");
            var response = await _client.PutAsync($"/api/messages/{id}", content);
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            var messageResult = JsonConvert.DeserializeObject<Hub.API.ViewModels.MessageViewModel>(body);

            messageResult.Title.Should().Be(newTestMessage.Title);
            messageResult.Description.Should().Be(newTestMessage.Description);
            messageResult.SortOrder.Should().Be(newTestMessage.SortOrder);
            messageResult.Id.Should().Be(id);
        }

        [Fact]
        public async Task Put_EndpointReturnSuccessWithDefaultsAndCorrectMessage()
        {
            testMessage.Id = Guid.Empty;
            var stringContent = new StringContent(JsonConvert.SerializeObject(testMessage), Encoding.UTF8, "application/json");
            var createResponse = await _client.PostAsync("/api/messages", stringContent);
            var createBody = await createResponse.Content.ReadAsStringAsync();
            var createdMessage = JsonConvert.DeserializeObject<Hub.API.ViewModels.MessageViewModel>(createBody);
            var id = createdMessage.Id;

            var title = "new title";
            var content = new StringContent(JsonConvert.SerializeObject(new { Title = title }), Encoding.UTF8, "application/json");
            var response = await _client.PutAsync($"/api/messages/{id}", content);
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            var messageResult = JsonConvert.DeserializeObject<Hub.API.ViewModels.MessageViewModel>(body);

            messageResult.Title.Should().Be(title);
            messageResult.Description.Should().Be(null);
            messageResult.SortOrder.Should().Be(0);
            messageResult.IsPublished.Should().BeFalse();
            messageResult.IsHighlighted.Should().BeFalse();
            messageResult.Id.Should().Be(id);
        }

        [Fact]
        public async Task Put_EndpointShouldRequireTitle()
        {
            testMessage.Id = Guid.Empty;
            var stringContent = new StringContent(JsonConvert.SerializeObject(testMessage), Encoding.UTF8, "application/json");
            var createResponse = await _client.PostAsync("/api/messages", stringContent);
            var createBody = await createResponse.Content.ReadAsStringAsync();
            var createdMessage = JsonConvert.DeserializeObject<Hub.API.ViewModels.MessageViewModel>(createBody);
            var id = createdMessage.Id;
            
            var content = new StringContent(JsonConvert.SerializeObject(new { }), Encoding.UTF8, "application/json");
            var response = await _client.PutAsync($"/api/messages/{id}", content);
            response.IsSuccessStatusCode.Should().BeFalse();
        }
    }
}

