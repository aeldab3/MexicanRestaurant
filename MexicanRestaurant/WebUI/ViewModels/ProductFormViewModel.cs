using System.ComponentModel.DataAnnotations;

namespace MexicanRestaurant.WebUI.ViewModels
{
    public class ProductFormViewModel
    {
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Product Name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
        public string? Name { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, int.MaxValue, ErrorMessage = "Price must be a non-negative number")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Stock is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Stock must be a non-negative number")]
        public int Stock { get; set; }

        [Required(ErrorMessage = "Please select a category")]
        public int CategoryId { get; set; }

        [DataType(DataType.Upload)]
        public IFormFile? ImageFile { get; set; }
        public string? ImageUrl { get; set; }
        public string? ExistingImageUrl { get; set; }
        public int[] SelectedIngredientIds { get; set; } = Array.Empty<int>();
    }
}
