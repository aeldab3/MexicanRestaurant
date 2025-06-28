using MexicanRestaurant.Core.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MexicanRestaurant.WebUI.ViewModels
{
    public class ProductListViewModel
    {
        public IEnumerable<Product> Products { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string SearchTerm { get; set; }
        public int? SelectedCategoryId { get; set; }
        public string SortBy { get; set; }
        public IEnumerable<SelectListItem> Categories { get; set; }
    }
}
