using MexicanRestaurant.Views.Shared;

namespace MexicanRestaurant.WebUI.ViewModels
{
    public class UserOrdersViewModel
    {
        public List<OrderListItemViewModel> Orders { get; set; }
        public PaginationInfo Pagination { get; set; }
    }
}
