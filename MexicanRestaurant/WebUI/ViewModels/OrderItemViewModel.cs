using System.ComponentModel.DataAnnotations;

namespace MexicanRestaurant.WebUI.ViewModels
{
    public class OrderItemViewModel
    {
        public int ProductId { get; set; }
        public ProductViewModel Product { get; set; }
        [Range(1, 100, ErrorMessage = "Quantity must be between 1 and 100")]
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}