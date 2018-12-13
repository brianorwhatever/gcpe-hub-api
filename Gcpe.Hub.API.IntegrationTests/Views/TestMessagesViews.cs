using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json;
using Xunit;

namespace Gcpe.Hub.API.IntegrationTests.Views
{
    public class TestMessagesViews : BaseWebApiTest
    {
        public StringContent testMessage = TestData.CreateMessage("Lorem Title", "Lorem description", 0, true, false);

        public TestMessagesViews(CustomWebApplicationFactory<Startup> factory) : base(factory)
        {
        }

        private async Task<Guid> _PostMessage()
        {
            var createResponse = await Client.PostAsync("/api/messages", testMessage);
            var createBody = await createResponse.Content.ReadAsStringAsync();
            var createdPost = JsonConvert.DeserializeObject<Models.Message>(createBody);
            return createdPost.Id;
        }

        [Fact]
        public async Task List_EndpointReturnSuccessAndCorrectMessagesSorted()
        {
            for (var i = 0; i < 5; i++)
            {
                int sortOrder = 5 - i;
                var newMessage = TestData.CreateMessage("Sorted Test Message", "test description", sortOrder, true, false);

                var createResponse = await Client.PostAsync("/api/messages", newMessage);
                createResponse.EnsureSuccessStatusCode();
            }

            var response = await Client.GetAsync("/api/messages");
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            var models = JsonConvert.DeserializeObject<Models.Message[]>(body).Where(m => m.Title == "Sorted Test Message");

            for (int i = 0; i < models.Count() - 1; i++)
            {
                models.ElementAt(i).SortOrder.Should().BeLessThan(models.ElementAt(i + 1).SortOrder);
            }
        }

        [Fact]
        public async Task Create_EndpointReturnSuccessAndCorrectMessage()
        {
            var newMessage = TestData.CreateMessage("Test title!", "test description!", 0, true, false);
            var createResponse = await Client.PostAsync("/api/messages", newMessage);
            createResponse.EnsureSuccessStatusCode();
            var createBody = await createResponse.Content.ReadAsStringAsync();
            var messageResult = JsonConvert.DeserializeObject<Models.Message>(createBody);

            messageResult.Title.Should().Be("Test title!");
            messageResult.Description.Should().Be("test description!");
        }

        [Fact]
        public async Task Create_EndpointRequiresTitle()
        {
            var brokenTestMessage = TestData.CreateMessage(null, "description", 0, true, false);
            var response = await Client.PostAsync("/api/messages", brokenTestMessage);
            response.IsSuccessStatusCode.Should().BeFalse();
        }

        [Fact]
        public async Task Create_EndpointDoesntRequireDescription()
        {
            var noDescMessage = TestData.CreateMessage("Title", null, 0, true, false);

            var response = await Client.PostAsync("/api/messages", noDescMessage);
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            var messageResult = JsonConvert.DeserializeObject<Models.Message>(body);

            messageResult.Title.Should().Be("Title");
            messageResult.Description.Should().BeNull();
        }

        [Fact]
        public async Task Get_EndpointReturnSuccessAndCorrectMessage()
        {
            Guid id = await _PostMessage();

            var response = await Client.GetAsync($"/api/Messages/{id}");
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            var messageResult = JsonConvert.DeserializeObject<Models.Message>(body);

            messageResult.Id.Should().Be(id);
        }

        [Fact]
        public async Task Get_EndpointReturnsNotFound()
        {
            var response = await Client.GetAsync($"/api/messages/{Guid.NewGuid()}");

            response.IsSuccessStatusCode.Should().BeFalse();
        }

        [Fact]
        public async Task Put_EndpointReturnSuccessAndCorrectMessage()
        {
            Guid id = await _PostMessage();
            var newTestMessage = TestData.CreateMessage("new title", "new description", 10, true, false);

            var response = await Client.PutAsync($"/api/messages/{id}", newTestMessage);
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            var messageResult = JsonConvert.DeserializeObject<Models.Message>(body);

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

            var response = await Client.PutAsync($"/api/messages/{id}", content);
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            var messageResult = JsonConvert.DeserializeObject<Models.Message>(body);

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
            var response = await Client.PutAsync($"/api/messages/{id}", content);
            response.IsSuccessStatusCode.Should().BeFalse();
        }
    }
}

