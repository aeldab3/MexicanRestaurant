using AutoMapper;
using MexicanRestaurant.Application.Helpers;
using MexicanRestaurant.Core.Interfaces;
using MexicanRestaurant.Core.Models;
using MexicanRestaurant.Views.Shared;
using MexicanRestaurant.WebUI.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace MexicanRestaurant.Application.Services
{
    public class PaginatedProductFetcher : IPaginatedProductFetcher
    {
        private readonly IRepository<Product> _products;
        private readonly IMapper _mapper;
        private readonly ILogger<PaginatedProductFetcher> _logger;
        public PaginatedProductFetcher(IRepository<Product> products, IMapper mapper, ILogger<PaginatedProductFetcher> logger)
        {
            _products = products;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<(List<ProductViewModel> Products, int TotalCount)> GetPagedProductsAsync(FilterOptionsViewModel filter, PaginationInfo pagination)
        {
            try
            {
                var query = _products.Table.Where(ProductFilteringHelper.BuildFilter(filter.SearchTerm, filter.SelectedCategoryId));
                query = ProductFilteringHelper.ApplyOrdering(query, filter.SortBy);

                var totalProducts = await query.CountAsync();

                var allProducts = await query
                    .AsNoTracking()
                    .Skip((pagination.CurrentPage - 1) * pagination.PageSize)
                    .Take(pagination.PageSize)
                    .Select(p => new ProductViewModel
                    {
                        ProductId = p.ProductId,
                        Name = p.Name ?? string.Empty,
                        Description = p.Description ?? string.Empty,
                        Price = p.Price,
                        Stock = p.Stock,
                        CategoryId = p.CategoryId,
                        CategoryName = p.Category !=null ? p.Category.Name : string.Empty,
                        ImageUrl = p.ImageUrl
                    })
                    .ToListAsync();
                return (allProducts, totalProducts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching paginated products");
                throw new ProductNotFoundException("An error occurred while fetching paginated products");
            }
        }
    }
}
