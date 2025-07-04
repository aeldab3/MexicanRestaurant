using AutoMapper;
using MexicanRestaurant.Application.Helpers;
using MexicanRestaurant.Core.Interfaces;
using MexicanRestaurant.Core.Models;
using MexicanRestaurant.Core.Specifications;
using MexicanRestaurant.Views.Shared;
using MexicanRestaurant.WebUI.ViewModels;

namespace MexicanRestaurant.Application.Services
{
    public class OrderViewModelFactory : IOrderViewModelFactory
    {
        private readonly IMapper _mapper;
        private readonly IRepository<Product> _products;
        private readonly IRepository<Category> _categories;
        private readonly ISessionService _sessionService;
        private readonly ISharedLookupService _sharedLookupService;

        private const string OrderSessionCartKey = "OrderViewModel";
        public OrderViewModelFactory(IRepository<Product> products, IMapper mapper, IRepository<Category> categories, ISessionService sessionService, ISharedLookupService sharedLookupService)
        {
            _products = products;
            _mapper = mapper;
            _categories = categories;
            _sessionService = sessionService;
            _sharedLookupService = sharedLookupService;
        }

        public async Task<OrderViewModel> InitializeOrderViewModelAsync(FilterOptionsViewModel filter, PaginationInfo pagination)
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
            var categories = await _sharedLookupService.GetCategorySelectListAsync();
            return new OrderViewModel
            {
                Products = mappedProducts,
                Filter = new FilterOptionsViewModel
                {
                    SearchTerm = filter.SearchTerm,
                    SelectedCategoryId = filter.SelectedCategoryId,
                    SortBy = filter.SortBy,
                    Categories = categories
                },
                Pagination = new PaginationInfo
                {
                    CurrentPage = pagination.CurrentPage,
                    PageSize = pagination.PageSize,
                    TotalPages = (int)Math.Ceiling((double)totalProducts / pagination.PageSize)
                },
                OrderItems = _sessionService.Get<OrderViewModel>(OrderSessionCartKey)?.OrderItems ?? new List<OrderItemViewModel>(),
            };
        }
    }
}
