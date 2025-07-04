using MexicanRestaurant.Core.Models;

namespace MexicanRestaurant.Core.Interfaces
{
    public interface IOrderProcessor
    {
        Task PlaceOrderAsync(string userId);
        Task<List<Order>> GetUserOrdersAsync(string userId);
    }
}
