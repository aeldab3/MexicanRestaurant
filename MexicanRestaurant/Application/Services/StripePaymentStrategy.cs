using MexicanRestaurant.Core.Interfaces;
using MexicanRestaurant.Core.Models;
using MexicanRestaurant.WebUI.ViewModels;
using Stripe;
using Stripe.Checkout;

namespace MexicanRestaurant.Application.Services
{
    public class StripePaymentStrategy : IPaymentStrategy
    {
        public async Task<PaymentResult> ProcessPaymentAsync(CheckoutViewModel CheckoutVM)
        {
            StripeConfiguration.ApiKey = Environment.GetEnvironmentVariable("StripeSecretKey") ?? "";
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(CheckoutVM.TotalAmount * 100),
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = "Order #" + CheckoutVM.OrderId
                            },
                        },
                        Quantity = 1,
                    }
                },
                Mode = "payment",
                SuccessUrl = CheckoutVM.SuccessUrl,
                CancelUrl = CheckoutVM.CancelUrl,
            };

            var service = new Stripe.Checkout.SessionService();
            Session session = await service.CreateAsync(options);

            return new PaymentResult
            {
                IsSuccess = true,
                PaymentUrl = session.Url
            };
        }
    }
}
