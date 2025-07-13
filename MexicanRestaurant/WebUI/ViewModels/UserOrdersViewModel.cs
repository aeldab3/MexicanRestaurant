using MexicanRestaurant.Core.Models;
using MexicanRestaurant.Views.Shared;

namespace MexicanRestaurant.WebUI.ViewModels
{
    public class UserOrdersViewModel
    {
        public List<Order> Orders { get; set; }
        public PaginationInfo Pagination { get; set; }
    }
}
