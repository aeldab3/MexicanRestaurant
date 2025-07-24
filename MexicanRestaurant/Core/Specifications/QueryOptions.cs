using System.Linq.Expressions;

namespace MexicanRestaurant.Core.Specifications
{
    public class QueryOptions<T> where T : class
    {
        public Expression<Func<T, bool>> Where { get; set; } = null!;
        public bool HasWhere => Where != null;
        private string[] includes = Array.Empty<string>();
        public string Includes
        {
            set => includes = value.Replace(" ", "").Split(",");
        }
        public string[] GetIncludes() => includes;
        public Func<IQueryable<T>, IOrderedQueryable<T>>? OrderByWithFunc { get; set; }
        public bool HasOrderByWithFunc => OrderByWithFunc != null;
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public bool DisablePaging { get; set; } = false;
        public bool HasPaging => PageNumber > 0 && PageSize > 0;
    }
}
