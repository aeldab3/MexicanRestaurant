using MexicanRestaurant.Core.Interfaces;
using MexicanRestaurant.Core.Models;
using Stripe;

namespace MexicanRestaurant.Application.Services
{
    public class StripePaymentStrategy : IPaymentStrategy
    {
        public async Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request)
        {
            StripeConfiguration.ApiKey = Environment.GetEnvironmentVariable("StripeSecretKey") ?? throw new InvalidOperationException("Stripe API key is missing");

            var paymentIntentService = new PaymentIntentService();
            try
            {
                var intentOptions = new PaymentIntentCreateOptions
                {
                    Amount = (long)(request.Amount * 100),
                    Currency = request.Currency.ToLower(),
                    Description = request.Description,
                    PaymentMethodTypes = new List<string> { "card" },
                    Metadata = new Dictionary<string, string>
                    {
                        { "OrderId", request.OrderId.ToString() },
                    }
                };

                var intent = await paymentIntentService.CreateAsync(intentOptions);
                return new PaymentResult
                {
                    IsSuccess = true,
                    Message = intent.ClientSecret,
                    PaymentIntentId = intent.Id
                };
            }
            catch (StripeException ex)
            {
                return new PaymentResult
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }
    }
}
