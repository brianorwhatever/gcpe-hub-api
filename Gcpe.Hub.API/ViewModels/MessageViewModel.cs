using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gcpe.Hub.API.ViewModels
{
    public class MessageViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int SortOrder { get; set; }
        public bool IsHighlighted { get; set; }
        public bool IsPublished { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
