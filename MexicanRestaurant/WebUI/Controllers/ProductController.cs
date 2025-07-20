using AutoMapper;
using MexicanRestaurant.Application.Helpers;
using MexicanRestaurant.Core.Interfaces;
using MexicanRestaurant.Core.Models;
using MexicanRestaurant.Views.Shared;
using MexicanRestaurant.WebUI.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MexicanRestaurant.WebUI.Controllers
{
    [Authorize]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ISharedLookupService _sharedLookupService;
        private readonly IMapper _mapper;
        public ProductController(IProductService productService, ISharedLookupService sharedLookupService, IMapper mapper)
        {
            _productService = productService;
            _sharedLookupService = sharedLookupService;
            _mapper = mapper;
        }

        [Authorize(Roles = "Admin, Manager")]
        [HttpGet]
        public async Task<IActionResult> Index(int page = 1, string searchTerm = "", int? categoryId = null, string sortBy = "")
        {
            var products = await _productService.GetPagedProductsAsync(
            new FilterOptionsViewModel { SearchTerm = searchTerm, SelectedCategoryId = categoryId, SortBy = sortBy },
            new PaginationInfo { CurrentPage = page, PageSize = 9 });
            return View(products);
        }

        [Authorize(Roles = "Admin, Manager")]
        [HttpGet]
        public async Task<IActionResult> AddEdit(int id)
        {
            await LoadFormData();

            if (id == 0)
            {
                ViewBag.Operation = "Add";
                return View(new ProductFormViewModel());
            }
            Product product = await _productService.GetExistingProductByIdAsync(id) 
                              ?? throw new ProductNotFoundException($"Product with ID {id} was not found.");
            var model = _mapper.Map<ProductFormViewModel>(product);
            model.ExistingImageUrl = product.ImageUrl;
            ViewBag.Operation = "Edit";
            return View(model);
        }

        [Authorize(Roles = "Admin, Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEdit(ProductFormViewModel model)
        {
                await LoadFormData();

                if (model.CategoryId == 0)
                    ModelState.AddModelError("CategoryId", "Please select a category.");

                if (ModelState.IsValid)
                {
                    var product = _mapper.Map<Product>(model);
                    product.ImageFile = model.ImageFile;
                    var existingImageUrl = model.ExistingImageUrl ?? string.Empty;
                    await _productService.AddOrUpdateProductAsync(product, model.SelectedIngredientIds, existingImageUrl);
                    TempData["Success"] = model.ProductId == 0 ? "Product added successfully." : "Product updated successfully.";
                    return RedirectToAction("Index");
                }
                ViewData["Error"] = "There were errors in the form. Please correct them and try again.";
                await LoadFormData();
                ViewBag.Operation = model.ProductId == 0 ? "Add" : "Edit";
                return View(model);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var model = await _productService.GetProductViewModelByIdAsync(id) 
                        ?? throw new ProductNotFoundException($"Product with ID {id} was not found.");
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productService.GetExistingProductByIdAsync(id)
                          ?? throw new ProductNotFoundException($"Product with ID {id} was not found.");
            await _productService.DeleteProductAsync(id);
            TempData["Success"] = "Product deleted successfully.";
            return RedirectToAction("Index");
        }

        private async Task LoadFormData()
        {
            ViewBag.Categories = await _sharedLookupService.GetCategorySelectListAsync();
            ViewBag.Ingredients = await _sharedLookupService.GetAllIngredientsAsync();
        }
    }
}
