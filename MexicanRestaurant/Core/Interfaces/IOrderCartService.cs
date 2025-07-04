using MexicanRestaurant.Core.Models;
using MexicanRestaurant.Views.Shared;
using MexicanRestaurant.WebUI.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Cryptography;

namespace MexicanRestaurant.Core.Interfaces
{
   public interface IOrderCartService
    {
        Task AddToOrderAsync(int productId, int productQantity);
        OrderViewModel GetCurrentOrderFromSession();
        void SaveCurrentOrderToSession(OrderViewModel model);
        Task IncreaseQuantityAsync(int productId);
        Task DecreaseQuantityAsync(int productId);
        Task RemoveFromOrderAsync(int productId);
    }
}
