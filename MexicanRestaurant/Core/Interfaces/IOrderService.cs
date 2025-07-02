using MexicanRestaurant.Core.Models;
using MexicanRestaurant.Views.Shared;
using MexicanRestaurant.WebUI.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Cryptography;

namespace MexicanRestaurant.Core.Interfaces
{
   public interface IOrderService
    {
        Task<OrderViewModel> InitializeOrderViewModelAsync(FilterOptionsViewModel filter, PaginationInfo pagination);
        Task AddItemToOrderAsync(int productId, int productQantity);
        OrderViewModel GetCurrentOrderFromSession();
        void SaveCurrentOrderToSession(OrderViewModel model);
        Task PlaceOrderAsync(string userId);
        Task<List<Order>> GetUserOrdersAsync(string userId);
        Task IncreaseItemQuantity(int productId);
        Task DecreaseItemQuantity(int productId);
        Task RemoveItemFromOrder(int productId);
        Task<List<SelectListItem>> GetCategorySelectListAsync();
    }
}
