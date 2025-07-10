using MexicanRestaurant.Core.Models;
using MexicanRestaurant.Views.Shared;
using MexicanRestaurant.WebUI.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MexicanRestaurant.Core.Interfaces
{
    public interface IProductService
    {
        Task<int> GetTotalProductsAsync();
        Task<Product> GetProductByIdAsync(int id);
        Task<Product> GetExistingProductByIdAsync(int id);
        Task AddOrUpdateProductAsync(Product product, int[] ingredientIds, string existingImageUrl);
        Task DeleteProductAsync(int id);
        Task<ProductListViewModel> GetPagedProductsAsync(FilterOptionsViewModel filter, PaginationInfo pagination);
    }
}
