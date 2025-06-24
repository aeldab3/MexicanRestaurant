using MexicanRestaurant.Core.Models;
using MexicanRestaurant.WebUI.ViewModels;
using System.Security.Cryptography;

namespace MexicanRestaurant.Core.Interfaces
{
   public interface IOrderService
    {
        Task<OrderViewModel> InitializeOrderViewModelAsync();
        Task AddItemToOrderAsync(int productId, int productQantity);
        OrderViewModel GetCurrentOrderFromSession();
        void SaveCurrentOrderToSession(OrderViewModel model);
        Task PlaceOrderAsync(string userId);
        Task<List<Order>> GetUserOrdersAsync(string userId);
        void IncreaseItemQuantity(int productId);
        void DecreaseItemQuantity(int productId);
        void RemoveItemFromOrder(int productId);
    }
}
