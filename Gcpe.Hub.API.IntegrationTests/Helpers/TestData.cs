using System;
using System.Net.Http;
using System.Text;
using Bogus;
using Newtonsoft.Json;

namespace Gcpe.Hub.API.IntegrationTests
{
    public static class TestData
    {
        private static Faker f = new Faker();

        public static StringContent SerializeObject(object o)
        {
            return new StringContent(JsonConvert.SerializeObject(o), Encoding.UTF8, "application/json");
        }

        public static StringContent CreatePost(string key)
        {
            return SerializeObject(new Models.Post
            {
                Key = key,
                Kind = "Story",
                Summary = string.Join(" ", f.Lorem.Words(3)),
                IsCommitted = true,
                Timestamp = f.Date.Past(),
                PublishDateTime = f.Date.Past(),
                AssetUrl = "test asset"
            });
        }

        public static StringContent CreateSocialMediaPost(string url, int sortOrder)
        {
            return SerializeObject(new Models.SocialMediaPost {
                SortOrder = sortOrder,
                Url = url,
                Timestamp = DateTime.Now
            });
        }

        public static StringContent CreateMessage(string title, string description,
            int sortOrder, bool isPublished = true, bool isHighlighted = false)
        {
            return SerializeObject(new Models.Message {
                Title = title,
                Description = description,
                SortOrder = sortOrder,
                IsPublished = isPublished,
                IsHighlighted = isHighlighted,
                Timestamp = DateTime.Now
            });
        }
    }
}
