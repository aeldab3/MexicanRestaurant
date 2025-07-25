namespace MexicanRestaurant.WebUI.ViewModels
{
    public class CheckoutResultViewModel
    {
        public int OrderId { get; set; }
        public string PaymentIntentId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
    }
}
