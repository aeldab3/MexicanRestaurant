using MexicanRestaurant.Core.Interfaces;
using MexicanRestaurant.Core.Models;
using MexicanRestaurant.Core.Specifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MexicanRestaurant.WebUI.Controllers
{
    [Authorize(Roles = "Admin, Manager")]
    public class IngredientController : Controller
    {
        private readonly IRepository<Ingredient> _ingredients;

        public IngredientController(IRepository<Ingredient> ingredients)
        {
            _ingredients = ingredients;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var ingredientList = await _ingredients.GetAllAsync();
            return View(ingredientList);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var ingredient = await _ingredients.GetByIdAsync(id, new QueryOptions<Ingredient>() { Includes = "ProductIngredients.Product"});
            if (ingredient == null) return NotFound();
            return View(ingredient);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IngredientId, Name")] Ingredient ingredient)
        {
            if (await _ingredients.ExistsAsync(i => i.Name.ToLower() ==  ingredient.Name.ToLower()))
            {
                TempData["ErrorMessage"] = "This ingredient already exists.";
                return View(ingredient);
            }

            if (ModelState.IsValid)
            {
                await _ingredients.AddAsync(ingredient);
                TempData["Success"] = "Ingredient created successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(ingredient);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var ingredient = await _ingredients.GetByIdAsync(id, new QueryOptions<Ingredient>() { Includes = "ProductIngredients.Product"});
            if (ingredient == null) return NotFound();
            return View(ingredient);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Ingredient ingredient)
        {
            if (id != ingredient.IngredientId) return NotFound();

            if (await _ingredients.ExistsAsync(i => i.Name.ToLower() == ingredient.Name.ToLower() && i.IngredientId != id))
            {
                TempData["ErrorMessage"] = "This ingredient already exists.";
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                await _ingredients.UpdateAsync(ingredient);
                TempData["Success"] = "Ingredient Edited successfully.";

                return RedirectToAction(nameof(Index));
            }
            return View(ingredient);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0) return NotFound();
            var ingredient = await _ingredients.GetByIdAsync(id, new QueryOptions<Ingredient>() { Includes = "ProductIngredients.Product"});
            if (ingredient == null) return NotFound();
            return View(ingredient);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (id <= 0) return NotFound();
            await _ingredients.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
