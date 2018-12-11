using System;
using System.ComponentModel.DataAnnotations;

namespace Gcpe.Hub.API.Models
{
    public class Message
    {
        public Guid Id { get; set; }
        [Required]
        public string Title { get; set; }
        public string Description { get; set; }
        public int SortOrder { get; set; }
        public bool IsHighlighted { get; set; }
        public bool IsPublished { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
