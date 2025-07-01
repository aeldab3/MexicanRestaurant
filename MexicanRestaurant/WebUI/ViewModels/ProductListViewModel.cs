using MexicanRestaurant.Core.Models;
using MexicanRestaurant.Views.Shared;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MexicanRestaurant.WebUI.ViewModels
    {
        public class ProductListViewModel
        {
            public IEnumerable<Product> Products { get; set; }
            public FilterOptionsViewModel Filter { get; set; }
            public PaginationInfo Pagination { get; set; }
        }
}
