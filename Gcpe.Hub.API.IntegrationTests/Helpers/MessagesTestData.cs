using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gcpe.Hub.API.ViewModels;
using Gcpe.Hub.Data.Entity;

namespace Gcpe.Hub.API.IntegrationTests.Helpers
{
    public class MessagesTestData
    {
        public static MessageViewModel CreateMessage(string title, string description,
            int sortOrder, bool isPublished = true, bool isHighlighted = false)
        {
            var message = new MessageViewModel
            {
                Id = Guid.Empty,
                Title = title,
                Description = description,
                SortOrder = sortOrder,
                IsPublished = isPublished,
                IsHighlighted = isHighlighted,
                Timestamp = DateTime.Now
            };

            return message;
        }
    }
}
