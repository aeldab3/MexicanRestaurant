using MexicanRestaurant.Core.Models;
using MexicanRestaurant.Views.Shared;
using MexicanRestaurant.WebUI.ViewModels;

namespace MexicanRestaurant.Core.Interfaces
{
    public interface IOrderProcessor
    {
        Task PlaceOrderAsync(string userId);
        Task<UserOrdersViewModel> GetPagedUserOrdersAsync(string userId, PaginationInfo pagination);
    }
}
