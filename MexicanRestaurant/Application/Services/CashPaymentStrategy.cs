using MexicanRestaurant.Core.Interfaces;
using MexicanRestaurant.Core.Models;
using MexicanRestaurant.WebUI.ViewModels;

namespace MexicanRestaurant.Application.Services
{
    public class CashPaymentStrategy : IPaymentStrategy
    {
        public Task<PaymentResult> ProcessPaymentAsync(CheckoutViewModel CheckoutVM)
        {
            return Task.FromResult(new PaymentResult
            {
                IsSuccess = true,
                PaymentUrl = null
            });
        }
    }
}
