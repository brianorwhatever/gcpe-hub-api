using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Gcpe.Hub.Data.Entity;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Gcpe.Hub.API.Helpers
{
    public static class Extensions
    {
        public static void AddPagination(this HttpResponse response,
            int currentPage, int itemsPerPage, int totalItems, int totalPages)
        {
            var paginationHeader = new PaginationHeader(currentPage, itemsPerPage, totalItems, totalPages);
            var camelCaseFormatter = new JsonSerializerSettings();
            camelCaseFormatter.ContractResolver = new CamelCasePropertyNamesContractResolver();
            response.Headers.Add("Pagination",
                JsonConvert.SerializeObject(paginationHeader, camelCaseFormatter));
            response.Headers.Add("Access-Control-Expose-Headers", "Pagination");

        }
        public static Models.Post ToModel(this NewsRelease dbPost, IMapper mapper)
        {
            var model = mapper.Map<Models.Post>(dbPost);

            var englishPost = dbPost.NewsReleaseLanguage.First(rl => rl.LanguageId == Language.enCA);
            model.Summary = englishPost.Summary;
            model.Location = englishPost.Location;
            var documents = new List<Models.Post.Document>();
            foreach (var document in dbPost.NewsReleaseDocument.OrderBy(e => e.SortIndex))
            {
                foreach (var documentLanguage in document.NewsReleaseDocumentLanguage)
                {
                    var documentModel = mapper.Map<Models.Post.Document>(documentLanguage);
                    var contacts = new List<Models.Post.Document.DocumentContact>();
                    foreach (var contact in documentLanguage.NewsReleaseDocumentContact)
                    {
                        var postDocumentContact = new Models.Post.Document.DocumentContact();
                        string[] lines = contact.Information.Replace("\r\n", "\n").Split('\n');
                        postDocumentContact.Title = lines[0];
                        postDocumentContact.Details = contact.Information.Substring(lines[0].Length).TrimStart();

                        contacts.Add(postDocumentContact);
                    }
                    documentModel.Contacts = contacts;
                    documents.Add(documentModel);
                }
            }
            model.LeadMinistryKey = dbPost.Ministry?.Key;
            model.LeadMinistryName = dbPost.Ministry?.DisplayName;
            model.Documents = documents;
            model.MinistryKeys = dbPost.NewsReleaseMinistry.Select(m => m.Release.Key).ToList();

            return model;
        }

        internal static void UpdateFromModel(this NewsRelease dbPost, Models.Post post, HubDbContext dbContext)
        {
            dbContext.Entry(dbPost).CurrentValues.SetValues(post);
            dbPost.ReleaseType = Enum.Parse<ReleaseType>(post.Kind);

            var newsReleaseLanguage = dbPost.NewsReleaseLanguage.FirstOrDefault();
            if (newsReleaseLanguage == null)
            {
                newsReleaseLanguage = new NewsReleaseLanguage() { LanguageId = Language.enCA };
                dbPost.NewsReleaseLanguage.Add(newsReleaseLanguage);
            }
            newsReleaseLanguage.Location = post.Location;
            newsReleaseLanguage.Summary = post.Summary;

            dbPost.Ministry = dbContext.Ministry.FirstOrDefault(m => m.Key == post.LeadMinistryKey);

            if (post.MinistryKeys != null)
            {
                dbContext.NewsReleaseMinistry.RemoveRange(dbPost.NewsReleaseMinistry.Where(m => !post.MinistryKeys.Contains(m.Ministry.Key)));

                foreach (var newMinistry in post.MinistryKeys.Where(sh => !dbPost.NewsReleaseMinistry.Any(m => m.Ministry.Key == sh)))
                {
                    dbPost.NewsReleaseMinistry.Add(new NewsReleaseMinistry { Release = dbPost, Ministry = dbContext.Ministry.Single(m => m.Key == newMinistry) });
                }
            }
            dbPost.Timestamp = DateTimeOffset.Now;
        }

        internal static void UpdateFromModel(this Activity dbActivity, Models.Activity activity, HubDbContext dbContext)
        {
            dbContext.Entry(dbActivity).CurrentValues.SetValues(activity);

            dbActivity.ContactMinistry = dbContext.Ministry.FirstOrDefault(m => m.Key == activity.ContactMinistryKey);

            if (activity.MinistriesSharedWith != null)
            {
                // This will also remove the unchecked ministries from dbActivity.ActivitySharedWith when the changed in the context are saved
                dbContext.ActivitySharedWith.RemoveRange(dbActivity.ActivitySharedWith.Where(m => !activity.MinistriesSharedWith.Contains(m.Ministry.Key)));

                foreach (var newMinistry in activity.MinistriesSharedWith.Where(sh => !dbActivity.ActivitySharedWith.Any(m => m.Ministry.Key == sh)))
                {
                    dbActivity.ActivitySharedWith.Add(new ActivitySharedWith { Activity = dbActivity, Ministry = dbContext.Ministry.Single(m => m.Key == newMinistry) });
                }
            }
            if (activity.Categories != null)
            {
                dbContext.ActivityCategories.RemoveRange(dbActivity.ActivityCategories.Where(ac => !activity.Categories.Contains(ac.Category.Name)));

                foreach (var newCategory in activity.Categories.Where(sh => !dbActivity.ActivityCategories.Any(ac => ac.Category.Name == sh)))
                {
                    dbActivity.ActivityCategories.Add(new ActivityCategories { Activity = dbActivity, Category = dbContext.Category.Single(m => m.Name == newCategory) });
                }
            }
            dbActivity.LastUpdatedDateTime = DateTime.Now;
        }
    }
}
