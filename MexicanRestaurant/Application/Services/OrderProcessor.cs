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
        private readonly IRepository<Order> _orderRepository;

        public OrderProcessor(IRepository<Order> orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<UserOrdersViewModel> GetPagedUserOrdersAsync(string userId, PaginationInfo pagination)
        {
            var options = new QueryOptions<Order>
            {
                Includes = "OrderItems.Product, DeliveryMethod",
                PageNumber = pagination.CurrentPage,
                PageSize = pagination.PageSize,
                Where = o => o.UserId == userId,
                OrderByWithFunc = o => o.OrderByDescending(o => o.OrderDate)
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
