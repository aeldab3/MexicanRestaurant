﻿namespace MexicanRestaurant.Application.Helpers
{
    public class ProductNotFoundException : Exception
    {
        public ProductNotFoundException(string message) : base(message) { }
    }
}
