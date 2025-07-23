using MexicanRestaurant.Core.Models;

namespace MexicanRestaurant.Core.Interfaces
{
    public interface IPaymentStrategy
    {
        Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request);
    }
}
