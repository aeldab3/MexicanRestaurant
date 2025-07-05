using MexicanRestaurant.Views.Shared;
using MexicanRestaurant.WebUI.ViewModels;

namespace MexicanRestaurant.Core.Interfaces
{
    public interface IPaginatedProductFetcher
    {
        Task<(List<ProductViewModel> Products, int TotalCount)> GetPagedProductsAsync(FilterOptionsViewModel filter, PaginationInfo pagination);
    }
}
