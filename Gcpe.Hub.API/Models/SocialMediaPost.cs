using System;
using System.ComponentModel.DataAnnotations;

namespace Gcpe.Hub.API.Models
{
    public class SocialMediaPost
    {
        public Guid Id { get; set; }
        [Required]
        public string Url { get; set; }
        public int SortOrder { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
