using MexicanRestaurant.WebUI.ViewModels;

namespace MexicanRestaurant.Core.Interfaces
{
    public interface ICheckoutService
    {
        Task<CheckoutViewModel> PrepareCheckoutAsync(string userId);
        Task ProcessCheckoutAsync(string userId, CheckoutViewModel model);
    }
}
