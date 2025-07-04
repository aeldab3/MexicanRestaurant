﻿using AutoMapper;
using MexicanRestaurant.Application.Services;
using MexicanRestaurant.Core.Interfaces;
using MexicanRestaurant.Core.Models;
using MexicanRestaurant.Views.Shared;
using MexicanRestaurant.WebUI.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace MexicanRestaurant.WebUI.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ISharedLookupService _sharedLookupService;
        private readonly IMapper _mapper;
        private readonly ILogger<ProductController> _logger;
        public ProductController(IProductService productService, ISharedLookupService sharedLookupService, IMapper mapper, ILogger<ProductController> logger)
        {
            _productService = productService;
            _sharedLookupService = sharedLookupService;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int page = 1, string searchTerm = "", int? categoryId = null, string sortBy = "")
        {
            var products = await _productService.GetPagedProductsAsync(
            new FilterOptionsViewModel { SearchTerm = searchTerm, SelectedCategoryId = categoryId, SortBy = sortBy },
            new PaginationInfo { CurrentPage = page, PageSize = 9 });
            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> AddEdit(int id)
        {
            ViewBag.Categories = await _sharedLookupService.GetCategorySelectListAsync();
            ViewBag.Ingredients = await _sharedLookupService.GetAllIngredientsAsync();

            if (id == 0)
            {
                ViewBag.Operation = "Add";
                return View(new ProductFormViewModel());
            }
            Product product = await _productService.GetExistingProductByIdAsync(id);
            if (product == null) return NotFound("Product not found");
            var model = _mapper.Map<ProductFormViewModel>(product);
            model.ExistingImageUrl = product.ImageUrl;
            ViewBag.Operation = "Edit";
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEdit(ProductFormViewModel model)
        {
            try
            {
                ViewBag.Categories = await _sharedLookupService.GetCategorySelectListAsync();
                ViewBag.Ingredients = await _sharedLookupService.GetAllIngredientsAsync();

                if (model.CategoryId == 0)
                    ModelState.AddModelError("CategoryId", "Please select a category.");

                if (ModelState.IsValid)
                {
                    var product = _mapper.Map<Product>(model);
                    product.ImageFile = model.ImageFile;
                    var existingImageUrl = model.ExistingImageUrl ?? string.Empty;
                    await _productService.AddOrUpdateProductAsync(product, model.SelectedIngredientIds, existingImageUrl);
                    return RedirectToAction("Index", "Product");
                }

                ViewBag.Categories = await _sharedLookupService.GetCategorySelectListAsync();
                ViewBag.Ingredients = await _sharedLookupService.GetAllIngredientsAsync();
                ViewBag.Operation = model.ProductId == 0 ? "Add" : "Edit";
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing your request.");
                TempData["ErrorMessage"] = "An error occurred while processing your request.";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try 
            { 
                var product = await _productService.GetProductByIdAsync(id);
                if (product == null) return NotFound("Product not found");
                var model = _mapper.Map<ProductViewModel>(product);
                if (model == null) return NotFound();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching product details for ID {id}.");
                TempData["ErrorMessage"] = "An error occurred while fetching product details.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _productService.DeleteProductAsync(id);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting product with ID {id}.");
                TempData["ErrorMessage"] = "An error occurred while deleting the product.";
                return RedirectToAction("Index");
            }
        }
    }
}
