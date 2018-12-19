using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Gcpe.Hub.API.Models
{
    public class Post
    {
        public class Document
        {
            public class DocumentContact
            {
                public string Title { get; set; }
                public string Details { get; set; }
            }

            public string PageTitle { get; set; }

            public int LanguageId { get; set; }

            public string Headline { get; set; }

            public string Subheadline { get; set; }

            public string BodyHtml { get; set; }

            public string Byline { get; set; }

            public IEnumerable<DocumentContact> Contacts { get; set; }
        }

        public string Kind { get; set; }
        public System.DateTimeOffset Timestamp { get; set; }
        [Required]
        [MaxLength(255)]
        public string Key { get; set; }
        public string Reference { get; set; }
        public string Summary { get; set; }
        public string Location { get; set; }

        public string LeadMinistryKey { get; set; }
        public string LeadMinistryName { get; set; }
        public IEnumerable<string> MinistryKeys { get; set; }

        public DateTimeOffset? PublishDateTime { get; set; }
        public bool IsCommitted { get; set; }
        public string AssetUrl { get; set; }
        public IEnumerable<Document> Documents { get; set; }
    }
}
