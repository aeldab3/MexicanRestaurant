using System.Linq.Expressions;

namespace MexicanRestaurant.Core.Specifications
{
    public class QueryOptions<T> where T : class
    {
        public Expression<Func<T, bool>> Where { get; set; } = null!;

        private string[] includes = Array.Empty<string>();
        public string Includes
        {
            set => includes = value.Replace(" ", "").Split(",");
        }
        public string[] GetIncludes() => includes;

        public bool HasWhere => Where != null;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 9;
        public bool DisablePaging { get; set; } = false;
        public bool HasOrderByWithFunc => OrderByWithFunc != null;
        public bool HasPaging => PageNumber > 0 && PageSize > 0;
        public Func<IQueryable<T>, IOrderedQueryable<T>>? OrderByWithFunc { get; set; }

    }
}
