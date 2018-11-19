using Gcpe.Hub.API.Helpers;
using Gcpe.Hub.API.Stubs;
using System.Collections.Generic;

namespace Gcpe.Hub.API.Data
{
    public interface IDataContext
    {
        List<NewsRelease> NewsReleases { get; set; }
        IEnumerable<NewsRelease> GetAll();
        IEnumerable<Article> GetArticles();
        NewsRelease Get(string id);
        void Add(object model);
        NewsRelease Update(string id, NewsRelease release);
        void Delete(NewsRelease release);
        IEnumerable<Article> GetSearchResults();
    }
}
