using Microsoft.AspNetCore.Mvc.Rendering;

namespace MexicanRestaurant.Views.Shared
{
    public class FilterOptionsViewModel
    {
        public string SearchTerm { get; set; }
        public string SortBy { get; set; }
        public int? SelectedCategoryId { get; set; }
        public IEnumerable<SelectListItem> Categories { get; set; }
    }
}
