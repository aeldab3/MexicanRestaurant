using AutoMapper;
using MexicanRestaurant.Application.Helpers;
using MexicanRestaurant.Core.Enums;
using MexicanRestaurant.Core.Interfaces;
using MexicanRestaurant.Core.Models;
using MexicanRestaurant.Core.Specifications;
using MexicanRestaurant.Views.Shared;
using MexicanRestaurant.WebUI.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace MexicanRestaurant.Application.Services
{
    public class OrderProcessor : IOrderProcessor
    {
        private readonly ISessionService _sessionService;
        private readonly IMapper _mapper;
        private readonly IRepository<Order> _orderRepository;
        private readonly ILogger<OrderProcessor> _logger;

        private const string OrderSessionCartKey = "OrderViewModel";
        public OrderProcessor(ISessionService sessionService, IMapper mapper, IRepository<Order> orderRepository, ILogger<OrderProcessor> logger)
        {
            _sessionService = sessionService;
            _mapper = mapper;
            _orderRepository = orderRepository;
            _logger = logger;
        }

        public async Task PlaceOrderAsync(string userId)
        {
            try 
            { 
                var model = _sessionService.Get<OrderViewModel>(OrderSessionCartKey);
                if (model == null || model.OrderItems.Count == 0) return;

                Order order = new Order
                {
                    UserId = userId,
                    TotalAmount = model.TotalAmount,
                    OrderDate = DateTime.Now,
                    OrderItems = _mapper.Map<List<OrderItem>>(model.OrderItems), 
                    Status = OrderStatus.Pending
                };
                await _orderRepository.AddAsync(order);
                _sessionService.Remove(OrderSessionCartKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error placing order");
                throw new ProductNotFoundException("An error occurred while placing the order");
            }

        }

        public async Task<UserOrdersViewModel> GetPagedUserOrdersAsync(string userId, PaginationInfo pagination)
        {
            var options = new QueryOptions<Order>
            {
                Includes = "OrderItems.Product",
                PageNumber = pagination.CurrentPage,
                PageSize = pagination.PageSize,
                Where = o => o.UserId == userId

            };

            var totalOrders = await _orderRepository.Table.CountAsync(o => o.UserId == userId);

            var orders = await _orderRepository.GetAllAsync(options);

            return new UserOrdersViewModel
            {
                Orders = orders.ToList(),
                Pagination = new PaginationInfo
                {
                    CurrentPage = pagination.CurrentPage,
                    PageSize = pagination.PageSize,
                    TotalPages = (int)Math.Ceiling((double)totalOrders / pagination.PageSize)
                }
            };
        }
    }
}
