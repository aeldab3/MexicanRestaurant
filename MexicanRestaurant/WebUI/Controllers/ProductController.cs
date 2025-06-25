using MexicanRestaurant.Core.Interfaces;
using MexicanRestaurant.Core.Models;
using MexicanRestaurant.Core.Services;
using MexicanRestaurant.Core.Specifications;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MexicanRestaurant.WebUI.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int page =1)
        {
            int pageSize = 9;
            var products = await _productService.GetPagedProductsAsync(page, pageSize);
            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> AddEdit(int id)
        {
            ViewBag.Categories = await _productService.GetCategorySelectListAsync();
            ViewBag.Ingredients = await _productService.GetAllIngredientsAsync();

            if (id == 0)
            {
                ViewBag.Operation = "Add";
                return View(new Product());
            }
            else
            {
                Product product = await _productService.GetExistingProductByIdAsync(id); 
                ViewBag.Operation = "Edit";
                return View(product);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEdit(Product product, int[] ingredientIds, string ExistingImageUrl)
        {
            ViewBag.Categories = await _productService.GetCategorySelectListAsync();

            ViewBag.Ingredients = await _productService.GetAllIngredientsAsync();

            if (product.CategoryId == 0)
                ModelState.AddModelError("CategoryId", "Please select a category.");

            if (ModelState.IsValid)
            {
                await _productService.AddOrUpdateProductAsync(product, ingredientIds, ExistingImageUrl);
                return RedirectToAction("Index", "Product");
            }

            ViewBag.Categories = await _productService.GetCategorySelectListAsync();
            ViewBag.Ingredients = await _productService.GetAllIngredientsAsync();
            ViewBag.Operation = product.ProductId == 0 ? "Add" : "Edit";

            return View(product);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);

            if (product == null)
                return NotFound();

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _productService.DeleteProductAsync(id);
            return RedirectToAction("Index");
        }
    }
}
