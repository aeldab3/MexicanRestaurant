using MexicanRestaurant.Core.Enums;

namespace MexicanRestaurant.Core.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public string? UserId { get; set; }
        public ApplicationUser User { get; set; }
        public decimal TotalAmount { get; set; }
        public Address ShippingAddress { get; set; }
        public int? DeliveryMethodId { get; set; }
        public DeliveryMethod DeliveryMethod { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public string PaymentMethod { get; set; }

    }
}
