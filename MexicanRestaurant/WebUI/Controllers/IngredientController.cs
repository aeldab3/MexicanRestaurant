using MexicanRestaurant.Core.Models;
using MexicanRestaurant.Infrastructure.Data;
using MexicanRestaurant.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;
using MexicanRestaurant.Core.Specifications;

namespace MexicanRestaurant.WebUI.Controllers
{
    public class IngredientController : Controller
    {
        private Repository<Ingredient> ingredients;
        public IngredientController(ApplicationDbContext context)
        {
            ingredients = new Repository<Ingredient>(context);
        }
        public async Task<IActionResult> Index()
        {
            var ingredientList = await ingredients.GetAllAsync();
            return View(ingredientList);
        }

        public async Task<IActionResult> Details(int id)
        {
            var ingredient = await ingredients.GetByIdAsync(id, new QueryOptions<Ingredient>() { Includes="ProductIngredients.Product"});
            if (ingredient == null)
            {
                return NotFound();
            }
            return View(ingredient);
        }
    }
}
