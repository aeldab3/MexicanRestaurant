using MexicanRestaurant.Views.Shared;
using MexicanRestaurant.WebUI.ViewModels;

namespace MexicanRestaurant.Core.Interfaces
{
    public interface IOrderViewModelFactory
    {
        Task<OrderViewModel> InitializeOrderViewModelAsync(FilterOptionsViewModel filter, PaginationInfo pagination);
    }
}
