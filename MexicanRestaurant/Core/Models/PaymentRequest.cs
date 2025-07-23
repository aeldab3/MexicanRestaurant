namespace MexicanRestaurant.Core.Models
{
    public class PaymentRequest
    {
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "usd";
        public string Description { get; set; }
        public string PaymentMethodId { get; set; }
    }
}
