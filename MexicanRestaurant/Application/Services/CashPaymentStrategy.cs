using MexicanRestaurant.Core.Interfaces;
using MexicanRestaurant.Core.Models;

namespace MexicanRestaurant.Application.Services
{
    public class CashPaymentStrategy : IPaymentStrategy
    {
        public Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request)
        {
            return Task.FromResult(new PaymentResult
            {
                IsSuccess = true,
                Message = "Cash payment processed successfully."
            });
        }
    }
}
