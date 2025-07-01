using System.ComponentModel.DataAnnotations;

namespace MexicanRestaurant.WebUI.ViewModels
{
    public class ProductFormViewModel
    {
        public int ProductId { get; set; }
        [Required(ErrorMessage = "Product Name is required")]
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        [Range(0, int.MaxValue)]
        public int Stock { get; set; }
        [Required(ErrorMessage = "Please select a category")]
        public int CategoryId { get; set; }
        public IFormFile? ImageFile { get; set; }
        public string? ImageUrl { get; set; }
        public string? ExistingImageUrl { get; set; }
        public int[] SelectedIngredientIds { get; set; } = Array.Empty<int>();
    }
}
