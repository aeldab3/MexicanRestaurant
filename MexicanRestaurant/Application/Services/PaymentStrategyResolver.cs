using MexicanRestaurant.Core.Interfaces;

namespace MexicanRestaurant.Application.Services
{
    public class PaymentStrategyResolver
    {
        private readonly IServiceProvider _serviceProvider;

        public PaymentStrategyResolver(IServiceProvider serviceProvider) 
        {
            _serviceProvider = serviceProvider;
        }

        public IPaymentStrategy Resolve(string method)
        {
            return method switch
            {
                "Card" => _serviceProvider.GetRequiredService<StripePaymentStrategy>(),
                "Cash" => _serviceProvider.GetRequiredService<CashPaymentStrategy>(),
                _ => throw new NotSupportedException($"Payment method '{method}' is not supported.")
            };
        }
    }
}
