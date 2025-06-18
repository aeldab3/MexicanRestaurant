using MexicanRestaurant.Core.Interfaces;
using MexicanRestaurant.Core.Models;
using MexicanRestaurant.Core.Specifications;
using MexicanRestaurant.Infrastructure.Data;
using MexicanRestaurant.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace MexicanRestaurant.WebUI.Controllers
{
    public class IngredientController : Controller
    {
        //private Repository<Ingredient> ingredients;
        private readonly IRepository<Ingredient> _ingredients;

        public IngredientController(IRepository<Ingredient> ingredients)
        {
            //ingredients = new Repository<Ingredient>(context);
            _ingredients = ingredients;
        }
        public async Task<IActionResult> Index()
        {
            var ingredientList = await _ingredients.GetAllAsync();
            return View(ingredientList);
        }

        public async Task<IActionResult> Details(int id)
        {
            var ingredient = await _ingredients.GetByIdAsync(id, new QueryOptions<Ingredient>() { Includes="ProductIngredients.Product"});
            if (ingredient == null)
            {
                return NotFound();
            }
            return View(ingredient);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IngredientId, Name")] Ingredient ingredient)
        {
            if (ModelState.IsValid)
            {
                await _ingredients.AddAsync(ingredient);
                return RedirectToAction(nameof(Index));
            }
            return View(ingredient);
        }
    }
}
