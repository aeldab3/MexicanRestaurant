using MexicanRestaurant.Views.Shared;
using MexicanRestaurant.WebUI.ViewModels;

namespace MexicanRestaurant.Core.Interfaces
{
    public interface IOrderProcessor
    {
        Task<UserOrdersViewModel> GetPagedUserOrdersAsync(string userId, PaginationInfo pagination);
    }
}
