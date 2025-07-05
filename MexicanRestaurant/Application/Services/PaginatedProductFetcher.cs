using AutoMapper;
using MexicanRestaurant.Application.Helpers;
using MexicanRestaurant.Core.Interfaces;
using MexicanRestaurant.Core.Models;
using MexicanRestaurant.Core.Specifications;
using MexicanRestaurant.Views.Shared;
using MexicanRestaurant.WebUI.ViewModels;

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
                var options = new QueryOptions<Product>
                {
                    Includes = nameof(Product.Category),
                    PageNumber = pagination.CurrentPage,
                    PageSize = pagination.PageSize,
                    Where = ProductFilteringHelper.BuildFilter(filter.SearchTerm, filter.SelectedCategoryId),
                    OrderByWithFunc = ProductFilteringHelper.BuildOrderBy(filter.SortBy)
                };
                var allProducts = await _products.GetAllAsync(options);
                var mappedProducts = _mapper.Map<List<ProductViewModel>>(allProducts);
                var countOptions = new QueryOptions<Product>
                {
                    Where = options.Where,
                    Includes = nameof(Product.Category),
                    DisablePaging = true
                };
                var totalProducts = (await _products.GetAllAsync(countOptions)).Count();
                return (mappedProducts, totalProducts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching paginated products");
                throw new ProductNotFoundException("An error occurred while fetching paginated products");
            }
        }
    }
}
