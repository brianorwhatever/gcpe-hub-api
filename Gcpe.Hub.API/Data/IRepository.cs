using System.Collections.Generic;
using Gcpe.Hub.API.Helpers;
using Gcpe.Hub.API.Stubs;

namespace Gcpe.Hub.API.Data
{
    public interface IRepository
    {
        void AddEntity(object model);
        IEnumerable<NewsRelease> GetAllReleases();
        IEnumerable<Article> GetAllArticles(SearchParams searchParams);
        IEnumerable<Article> GetSearchResults();
        NewsRelease Update(string id, NewsRelease release);
        NewsRelease GetReleaseByKey(string key);
        void Delete(NewsRelease release);
    }
}
