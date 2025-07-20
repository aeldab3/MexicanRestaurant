using AutoMapper;
using AutoMapper.QueryableExtensions;
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
        public PaginatedProductFetcher(IRepository<Product> products, IMapper mapper)
        {
            _products = products;
            _mapper = mapper;
        }

        public async Task<(List<ProductViewModel> Products, int TotalCount)> GetPagedProductsAsync(FilterOptionsViewModel filter, PaginationInfo pagination)
        {
            var query = _products.Table.Where(ProductFilteringHelper.BuildFilter(filter.SearchTerm, filter.SelectedCategoryId));
            query = ProductFilteringHelper.ApplyOrdering(query, filter.SortBy);

            var totalProducts = await query.CountAsync();

            var products = query
                .AsNoTracking()
                .Skip((pagination.CurrentPage - 1) * pagination.PageSize)
                .Take(pagination.PageSize);
            
            var allProducts = await products.ProjectTo<ProductViewModel>(_mapper.ConfigurationProvider).ToListAsync();
            return (allProducts, totalProducts);
        }
    }
}
