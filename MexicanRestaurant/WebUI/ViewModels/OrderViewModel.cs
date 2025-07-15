using MexicanRestaurant.Core.Models;
using MexicanRestaurant.Views.Shared;

namespace MexicanRestaurant.WebUI.ViewModels
{
    public class OrderViewModel
    {
        public decimal TotalAmount { get; set; }
        public List<OrderItemViewModel> OrderItems { get; set; } = new List<OrderItemViewModel>();
        public List<ProductViewModel> Products { get; set; } = new List<ProductViewModel>();
        public FilterOptionsViewModel Filter { get; set; }
        public PaginationInfo Pagination { get; set; }
        public ShippingAddressViewModel ShippingAddress { get; set; } = new ShippingAddressViewModel();
        public int DeliveryMethodId { get; set; }
        public List<DeliveryMethod>? AvailableDeliveryMethods { get; set; }
    }
}
