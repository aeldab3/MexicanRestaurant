using MexicanRestaurant.Core.Models;
using MexicanRestaurant.Views.Shared;

namespace MexicanRestaurant.WebUI.ViewModels
{
    public class ProductListViewModel
    {
        public IEnumerable<ProductViewModel> Products { get; set; }
        public FilterOptionsViewModel Filter { get; set; }
        public PaginationInfo Pagination { get; set; }
    }
}
