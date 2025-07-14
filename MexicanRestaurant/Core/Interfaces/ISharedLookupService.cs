using MexicanRestaurant.Core.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MexicanRestaurant.Core.Interfaces
{
    public interface ISharedLookupService
    {
        Task<List<SelectListItem>> GetCategorySelectListAsync();
        Task<IEnumerable<Ingredient>> GetAllIngredientsAsync();
        Task<List<DeliveryMethod>> GetAllDeliveryMethodsAsync();
    }
}
