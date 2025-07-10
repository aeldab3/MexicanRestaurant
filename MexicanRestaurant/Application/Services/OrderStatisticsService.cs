using MexicanRestaurant.Core.Interfaces;
using MexicanRestaurant.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace MexicanRestaurant.Application.Services
{
    public class OrderStatisticsService : IOrderStatisticsService
    {
        private readonly IRepository<Order> _orderRepository;
        public OrderStatisticsService(IRepository<Order> orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<int> GetTotalOrdersAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            return await FilterOrders(startDate, endDate).CountAsync();
        }

        public async Task<decimal> GetTotalRevenueAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            return await FilterOrders(startDate, endDate).SumAsync(o => o.TotalAmount);
        }

        public async Task<List<string>> GetTopProductNamesAsync(int top = 5, DateTime? startDate = null, DateTime? endDate = null)
        {
            return await FilterOrders(startDate, endDate)
                .SelectMany(o => o.OrderItems)
                .GroupBy(i => i.Product.Name) 
                .OrderByDescending(g => g.Sum(i => i.Quantity))
                .Take(top)
                .Select(g => g.Key)
                .ToListAsync();
        }

        public async Task<List<int>> GetTopProductSalesAsync(int top = 5, DateTime? startDate = null, DateTime? endDate = null)
        {
            return await FilterOrders(startDate, endDate)
                .SelectMany(o => o.OrderItems) 
                .GroupBy(i => i.Product.Name) 
                .OrderByDescending(g => g.Sum(i => i.Quantity))
                .Take(top)
                .Select(g => g.Sum(i => i.Quantity))
                .ToListAsync();
        }

        public async Task<Dictionary<string, decimal>> GetRevenueByDateAsync(DateTime? startDate, DateTime? endDate)
        {
            var orders = await FilterOrders(startDate, endDate).ToListAsync();
            return orders
                .GroupBy(o => o.OrderDate.Date)
                .OrderBy(g => g.Key)
                .ToDictionary(
                    g => g.Key.ToString("yyyy-MM-dd"),
                    g => g.Sum(o => o.TotalAmount)
                );
        }

        private IQueryable<Order> FilterOrders(DateTime? startDate, DateTime? endDate)
        {
            var query = _orderRepository.Table.Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product) 
            .AsQueryable();

            if (startDate.HasValue)
                query = query.Where(o => o.OrderDate.Date >= startDate.Value.Date);

            if (endDate.HasValue)
                query = query.Where(o => o.OrderDate.Date < endDate.Value.Date.AddDays(1));

            return query;
        }
    }
}
