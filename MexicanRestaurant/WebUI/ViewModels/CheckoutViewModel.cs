using MexicanRestaurant.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace MexicanRestaurant.WebUI.ViewModels
{
    public class CheckoutViewModel
    {
        public int OrderId { get; set; }
        [Required]
        public ShippingAddressViewModel ShippingAddress { get; set; } = new ShippingAddressViewModel();

        [Required]
        [Display(Name = "Delivery Method")]
        public int SelectedDeliveryMethodId { get; set; }
        public List<DeliveryMethod> AvailableDeliveryMethods { get; set; } = new();
        public decimal TotalAmount { get; set; }
        public List<OrderItemViewModel> OrderItems { get; set; } = new();
        public List<string> AvailablePaymentMethods { get; set; } = new List<string> {"Cash", "Card"};

        [Required]
        public string SelectedPaymentMethod { get; set; }
        public string SuccessUrl { get; set; } = "https://localhost:7063/Order/CheckoutSuccess";
        public string CancelUrl { get; set; } = "https://localhost:7063/Order/CheckoutCancel";
        public string StripePublishableKey { get; set; }
        public decimal OrderTotal { get; set; }
        public string ClientSecret { get; set; }

    }
}
