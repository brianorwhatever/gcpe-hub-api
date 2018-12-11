using System.ComponentModel.DataAnnotations;

namespace Gcpe.Hub.API.Models
{
    public class NewsReleaseLog
    {
        public System.DateTimeOffset DateTime { get; set; }
        [Required]
        [MaxLength(255)]
        public string Description { get; set; }

        public string ReleaseKey { get; set; }
        public string UserName { get; set; }
    }
}
