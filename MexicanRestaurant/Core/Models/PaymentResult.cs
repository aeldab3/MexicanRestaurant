namespace MexicanRestaurant.Core.Models
{
    public class PaymentResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public string PaymentIntentId { get; set; }
        public string ClientSecret { get; set; }
    }
}
