﻿using MexicanRestaurant.Core.Models;

namespace MexicanRestaurant.WebUI.ViewModels
{
    public class ProductListViewModel
    {
        public IEnumerable<Product> Products { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}
