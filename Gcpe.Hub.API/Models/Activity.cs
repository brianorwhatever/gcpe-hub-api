using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Gcpe.Hub.API.Models
{
    public class Activity
    {
        public int Id { get; set; }
        public DateTime? StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
        public string PotentialDates { get; set; }
        public string Title { get; set; }
        [Required]
        [StringLength(500)]
        public string Details { get; set; }
        [Required]
        [StringLength(500)]
        public string Schedule { get; set; }
        [Required]
        [StringLength(500)]
        public string Significance { get; set; }
        [StringLength(500)]
        public string Strategy { get; set; }
        [StringLength(4000)]
        public string Comments { get; set; }
        [StringLength(2000)]
        public string LeadOrganization { get; set; }
        public string ContactMinistryKey { get; set; }
        [StringLength(150)]
        public string Venue { get; set; }
        public string CityName { get; set; }
        [StringLength(150)]
        public string OtherCity { get; set; }
        public bool IsActive { get; set; }
        public bool IsConfirmed { get; set; }
        public bool IsIssue { get; set; }
        public bool IsAllDay { get; set; }
        public DateTime? NrDateTime { get; set; }
        public DateTime? LastUpdatedDateTime { get; set; }

        public ICollection<string> Categories { get; set; }
        public ICollection<string> MinistriesSharedWith { get; set; }
    }
}
