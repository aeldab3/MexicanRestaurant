namespace MexicanRestaurant.Core.Interfaces
{
    public interface IOrderStatisticsService
    {
        Task<int> GetTotalOrdersAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<decimal> GetTotalRevenueAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<List<string>> GetTopProductNamesAsync(int top = 5, DateTime? startDate = null, DateTime? endDate = null);
        Task<List<int>> GetTopProductSalesAsync(int top = 5, DateTime? startDate = null, DateTime? endDate = null);
        Task<Dictionary<string, decimal>> GetRevenueByDateAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<Dictionary<string, int>> GetCategorySalesAsync(DateTime? startDate = null, DateTime? endDate = null);
    }
}
