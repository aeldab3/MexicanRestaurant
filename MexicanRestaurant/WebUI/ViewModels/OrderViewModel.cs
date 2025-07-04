﻿using MexicanRestaurant.Views.Shared;

namespace MexicanRestaurant.WebUI.ViewModels
{
    public class OrderViewModel
    {
        public decimal TotalAmount { get; set; }
        public List<OrderItemViewModel> OrderItems { get; set; }
        public List<ProductViewModel> Products { get; set; }
        public FilterOptionsViewModel Filter { get; set; }
        public PaginationInfo Pagination { get; set; }
    }
}
