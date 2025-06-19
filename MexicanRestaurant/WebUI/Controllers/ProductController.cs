using MexicanRestaurant.Core.Interfaces;
using MexicanRestaurant.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace MexicanRestaurant.WebUI.Controllers
{
    public class ProductController : Controller
    {
        private readonly IRepository<Product> _products;
        private readonly IRepository<Category> _categories;
        private readonly IRepository<Ingredient> _ingredients;
        public ProductController(IRepository<Product> products, IRepository<Category> categories, IRepository<Ingredient> ingredients)
        {
            _products = products;
            _categories = categories;
            _ingredients = ingredients;
        }
        public async Task<IActionResult> Index()
        {
            var products = await _products.GetAllAsync();
            return View(products);
        }

        public async Task<IActionResult> AddEdit(int id)
        {
            ViewBag.Categories = await _categories.GetAllAsync();
            ViewBag.Ingredients = await _ingredients.GetAllAsync();

            if (id ==0 )
            {
                ViewBag.Operation = "Add";
                return View(new Product());
            }
            else
            {
                ViewBag.Operation = "Edit";
                return View();
            }
        }

    }
}
