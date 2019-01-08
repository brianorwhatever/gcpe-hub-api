using System.Collections.Generic;
using System.Linq;

namespace Gcpe.Hub.API.Helpers
{
    public class PagedList<T> : List<T>
    {
        public static IEnumerable<T> Create(IQueryable<T> source,
            int pageNumber, int pageSize)
        {
            return source.Skip((pageNumber - 1) * pageSize).Take(pageSize);
        }
    }
}
