using MexicanRestaurant.Application.Helpers;
using MexicanRestaurant.Core.Interfaces;
using MexicanRestaurant.Views.Shared;
using MexicanRestaurant.WebUI.ViewModels;

namespace MexicanRestaurant.Application.Services
{
    public class OrderViewModelFactory : IOrderViewModelFactory
    {
        private readonly ISessionService _sessionService;
        private readonly ISharedLookupService _sharedLookupService;
        private readonly IPaginatedProductFetcher _paginatedProductFetcher;
        private readonly ILogger<OrderViewModelFactory> _logger;

        private const string OrderSessionCartKey = "OrderViewModel";
        public OrderViewModelFactory(ISessionService sessionService, ISharedLookupService sharedLookupService, IPaginatedProductFetcher paginatedProductFetcher, ILogger<OrderViewModelFactory> logger)
        {
            _sessionService = sessionService;
            _sharedLookupService = sharedLookupService;
            _paginatedProductFetcher = paginatedProductFetcher;
            _logger = logger;
        }

        public async Task<OrderViewModel> InitializeOrderViewModelAsync(FilterOptionsViewModel filter, PaginationInfo pagination)
        {
            try 
            { 
                var (mappedProducts, totalProducts) = await _paginatedProductFetcher.GetPagedProductsAsync(filter, pagination);
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing OrderViewModel");
                throw new ProductNotFoundException("An error occurred while initializing the order view model");
            }
        }
    }
}
