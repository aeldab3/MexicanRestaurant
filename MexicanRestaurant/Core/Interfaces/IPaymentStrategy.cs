using MexicanRestaurant.Core.Models;
using MexicanRestaurant.WebUI.ViewModels;

namespace MexicanRestaurant.Core.Interfaces
{
    public interface IPaymentStrategy
    {
        Task<PaymentResult> ProcessPaymentAsync(CheckoutViewModel CheckoutVM);
    }
}
