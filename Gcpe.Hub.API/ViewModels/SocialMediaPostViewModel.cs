using System;
using System.ComponentModel.DataAnnotations;

namespace Gcpe.Hub.API.ViewModels
{
    public class SocialMediaPostViewModel
    {
        public Guid Id { get; set; }
        [Required]
        public string Url { get; set; }
        public int SortOrder { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
