using AutoMapper;
using MexicanRestaurant.Application.Helpers;
using MexicanRestaurant.Core.Interfaces;
using MexicanRestaurant.Core.Models;
using MexicanRestaurant.Core.Specifications;
using MexicanRestaurant.WebUI.ViewModels;

namespace MexicanRestaurant.Application.Services
{
    public class OrderProcessor : IOrderProcessor
    {
        private readonly ISessionService _sessionService;
        private readonly IMapper _mapper;
        private readonly IRepository<Order> _orders;
        private readonly ILogger<OrderProcessor> _logger;

        private const string OrderSessionCartKey = "OrderViewModel";
        public OrderProcessor(ISessionService sessionService, IMapper mapper, IRepository<Order> orders, ILogger<OrderProcessor> logger)
        {
            _sessionService = sessionService;
            _mapper = mapper;
            _orders = orders;
            _logger = logger;
        }

        public async Task PlaceOrderAsync(string userId)
        {
            try { 
                var model = _sessionService.Get<OrderViewModel>(OrderSessionCartKey);
                if (model == null || model.OrderItems.Count == 0) return;

                Order order = new Order
                {
                    UserId = userId,
                    TotalAmount = model.TotalAmount,
                    OrderDate = DateTime.Now,
                    OrderItems = _mapper.Map<List<OrderItem>>(model.OrderItems)
                };
                await _orders.AddAsync(order);
                _sessionService.Remove(OrderSessionCartKey);
                }
                catch (Exception ex)
            {
                _logger.LogError(ex, "Error placing order");
                throw new ProductNotFoundException("An error occurred while placing the order");
            }

        }

        public async Task<List<Order>> GetUserOrdersAsync(string userId)
        {
            return (await _orders.GetAllByIdAsync(userId, "UserId", new QueryOptions<Order>
            {
                Includes = "OrderItems.Product",
                DisablePaging = true
            })).ToList();
        }
    }
}
