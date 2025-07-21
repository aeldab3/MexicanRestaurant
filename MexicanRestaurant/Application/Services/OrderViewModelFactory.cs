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

        private const string OrderSessionCartKey = "OrderViewModel";
        public OrderViewModelFactory(ISessionService sessionService, ISharedLookupService sharedLookupService, IPaginatedProductFetcher paginatedProductFetcher)
        {
            _sessionService = sessionService;
            _sharedLookupService = sharedLookupService;
            _paginatedProductFetcher = paginatedProductFetcher;
        }

        public async Task<OrderViewModel> InitializeOrderViewModelAsync(FilterOptionsViewModel filter, PaginationInfo pagination)
        {
            var (mappedProducts, totalProducts) = await _paginatedProductFetcher.GetPagedProductsAsync(filter, pagination);
            var categories = await _sharedLookupService.GetCategorySelectListAsync();
            var deliveryMethods = await _sharedLookupService.GetAllDeliveryMethodsAsync();
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
                AvailableDeliveryMethods = deliveryMethods,
                OrderItems = _sessionService.Get<OrderViewModel>(OrderSessionCartKey)?.OrderItems ?? new List<OrderItemViewModel>(),
            };
        }
    }
}
