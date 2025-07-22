namespace MexicanRestaurant.Core.Models
{
    public class PaymentResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public string PaymentUrl { get; set; } = string.Empty;
    }
}
