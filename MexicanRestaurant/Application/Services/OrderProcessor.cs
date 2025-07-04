using AutoMapper;
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

        private const string OrderSessionCartKey = "OrderViewModel";
        public OrderProcessor(ISessionService sessionService, IMapper mapper, IRepository<Order> orders)
        {
            _sessionService = sessionService;
            _mapper = mapper;
            _orders = orders;
        }

        public async Task PlaceOrderAsync(string userId)
        {
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
