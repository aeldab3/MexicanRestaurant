using MexicanRestaurant.Core.Extensions;
using MexicanRestaurant.Core.Models;
using Microsoft.Extensions.Options;
using System.Linq.Expressions;

namespace MexicanRestaurant.Application.Helpers
{
    public static class ProductFilteringHelper
    {
        public static Expression<Func<Product, bool>> BuildFilter(string? searchTerm, int? categoryId)
        {
            Expression<Func<Product, bool>> filter = p => true;

            if (!string.IsNullOrEmpty(searchTerm))
                filter = filter.AndAlso(p => p.Name.Contains(searchTerm.ToLower()) || p.Description.Contains(searchTerm.ToLower()));

            if (categoryId.HasValue && categoryId.Value > 0)
                filter = filter.AndAlso(p => p.CategoryId == categoryId.Value);

            return filter;
        }

        public static Func<IQueryable<Product>, IOrderedQueryable<Product>> BuildOrderBy(string? sortBy)
        {
            return sortBy switch
            {
                "name_asc" => p => p.OrderBy(p => p.Name),
                "name_desc" => p => p.OrderByDescending(p => p.Name),
                "price_asc" => p => p.OrderBy(p => p.Price),
                "price_desc" => p => p.OrderByDescending(p => p.Price),
                _ => p => p.OrderBy(p => p.ProductId),
            };
        }

        public static IQueryable<Product> ApplyOrdering(IQueryable<Product> query, string? sortBy)
        {
            var orderBy = BuildOrderBy(sortBy);
            return orderBy(query);
        }
    }
}
