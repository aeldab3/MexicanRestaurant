using MexicanRestaurant.Core.Models;
using MexicanRestaurant.Views.Shared;
using MexicanRestaurant.WebUI.ViewModels;

namespace MexicanRestaurant.Core.Interfaces
{
    public interface IProductService
    {
        Task<int> GetTotalProductsAsync();
        Task<Product> GetExistingProductByIdAsync(int id);
        Task<ProductViewModel?> GetProductViewModelByIdAsync(int id);
        Task AddOrUpdateProductAsync(Product product, int[] ingredientIds, string existingImageUrl);
        Task DeleteProductAsync(int id);
        Task<ProductListViewModel> GetPagedProductsAsync(FilterOptionsViewModel filter, PaginationInfo pagination);
    }
}
