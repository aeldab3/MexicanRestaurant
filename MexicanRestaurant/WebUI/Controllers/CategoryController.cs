using MexicanRestaurant.Application.Helpers;
using MexicanRestaurant.Core.Interfaces;
using MexicanRestaurant.Core.Models;
using MexicanRestaurant.Core.Specifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MexicanRestaurant.WebUI.Controllers
{
    [Authorize(Roles = "Admin, Manager")]
    public class CategoryController : Controller
    {
        private readonly IRepository<Category> _categories;
        private readonly IAuditLogHelper _auditLogHelper;

        public CategoryController(IRepository<Category> categories, IAuditLogHelper auditLogHelper)
        {
            _categories = categories;
            _auditLogHelper = auditLogHelper;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var categories = await _categories.GetAllAsync();
            return View(categories);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var category = await _categories.GetByIdAsync(id, new QueryOptions<Category> { Includes = "Products" })
                ?? throw new KeyNotFoundException($"Category with ID {id} was not found.");
            return View(category);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CategoryId, Name")] Category category)
        {
            if (await _categories.ExistsAsync(c => c.Name.ToLower() == category.Name.ToLower()))
            {
                TempData["Error"] = "A category with this name already exists.";
                return View(category);
            }

            if (ModelState.IsValid)
            {
                await _categories.AddAsync(category);
                await _auditLogHelper.LogActionAsync(HttpContext, "Create", "Category", category.CategoryId.ToString(), $"Created category: {category.Name}");
                TempData["Success"] = "Category created successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _categories.GetByIdAsync(id, new QueryOptions<Category>() { Includes = "Products" })
                ?? throw new KeyNotFoundException($"Category with ID {id} was not found.");
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category category)
        {
            if (id != category.CategoryId)
                return NotFound();

            if (await _categories.ExistsAsync(c => c.Name.ToLower() == category.Name.ToLower() && c.CategoryId != id))
            {
                TempData["Error"] = "A category with this name already exists.";
                return View(category);
            }

            if (ModelState.IsValid)
            {
                await _categories.UpdateAsync(category);
                await _auditLogHelper.LogActionAsync(HttpContext, "Update", "Category", category.CategoryId.ToString(), $"Updated category: {category.Name}");
                TempData["Success"] = "Category updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return NotFound();

            var category = await _categories.GetByIdAsync(id, new QueryOptions<Category> { Includes = "Products" })
                ?? throw new KeyNotFoundException($"Category with ID {id} was not found.");
            if (category.Products.Any())
            {
                TempData["Error"] = "Cannot delete a category that has products associated with it.";
                return RedirectToAction(nameof(Index));
            }

            await _categories.DeleteAsync(id);
            await _auditLogHelper.LogActionAsync(HttpContext, "Delete", "Category", id.ToString(), $"Deleted category: {category.Name}");
            TempData["Success"] = "Category deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
