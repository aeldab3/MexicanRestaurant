using MexicanRestaurant.Core.Interfaces;
using MexicanRestaurant.Core.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MexicanRestaurant.Application.Services
{
    public class SharedLookupService : ISharedLookupService
    {
        private readonly IRepository<Category> _categories;
        private readonly IRepository<Ingredient> _ingredients;
        public SharedLookupService(IRepository<Category> categories, IRepository<Ingredient> ingredients)
        {
            _categories = categories;
            _ingredients = ingredients;
        }

        public async Task<List<SelectListItem>> GetCategorySelectListAsync()
        {
            var categories = await _categories.GetAllAsync();
            return categories.Select(c => new SelectListItem
            {
                Value = c.CategoryId.ToString(),
                Text = c.Name
            }).ToList();
        }

        public async Task<IEnumerable<Ingredient>> GetAllIngredientsAsync()
        {
            return await _ingredients.GetAllAsync();
        }
    }
}
