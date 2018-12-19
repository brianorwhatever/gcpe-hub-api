using System.ComponentModel.DataAnnotations;

namespace Gcpe.Hub.API.Models
{
    public class PostLog
    {
        public System.DateTimeOffset DateTime { get; set; }
        [Required]
        [MaxLength(255)]
        public string Description { get; set; }

        public string PostKey { get; set; }
        public string UserName { get; set; }
    }
}
