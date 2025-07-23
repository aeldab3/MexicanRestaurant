using MexicanRestaurant.Core.Enums;

namespace MexicanRestaurant.WebUI.ViewModels
{
    public class OrderListItemViewModel
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string DeliveryShortName { get; set; }
        public OrderStatus Status { get; set; }
        public string PaymentMethod { get; set; }
        public ShippingAddressViewModel ShippingAddress { get; set; }
        public List<OrderItemViewModel> OrderItems { get; set; }
        public string UserFullName { get; set; }
        public string UserEmail { get; set; }
    }
}
