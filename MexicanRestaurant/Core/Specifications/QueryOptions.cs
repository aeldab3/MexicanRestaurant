using System.Linq.Expressions;

namespace MexicanRestaurant.Core.Specifications
{
    public class QueryOptions<T> where T : class
    {
        public Expression<Func<T, object>> OrderBy { get; set; } = null!;
        public Expression<Func<T, bool>> Where { get; set; } = null!;

        private string[] includes = Array.Empty<string>();
        public string Includes
        {
            set => includes = value.Replace(" ", "").Split(",");
        }
        public string[] GetIncludes() => includes;

        public bool HasWhere => Where != null;
        public bool HasOrderBy => OrderBy != null;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 9;
        public bool HasPaging => PageNumber > 0 && PageSize > 0;
        public bool IsDescending { get; set; } = false;

    }
}
