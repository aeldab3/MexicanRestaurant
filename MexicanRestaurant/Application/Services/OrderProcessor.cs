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
            var query = _orderRepository.Table
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .Skip((pagination.CurrentPage - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .Select(o => new OrderListItemViewModel
                {
                    OrderId = o.OrderId,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status,
                    DeliveryShortName = o.DeliveryMethod.ShortName,
                    ShippingAddress = new ShippingAddressViewModel
                    {
                        Street = o.ShippingAddress.Street,
                        City = o.ShippingAddress.City,
                        State = o.ShippingAddress.State
                    },
                    OrderItems = o.OrderItems.Select(oi => new OrderItemViewModel
                    {
                        Product = new ProductViewModel
                        {
                            Name = oi.Product!.Name!,
                        },
                        Quantity = oi.Quantity,
                        Price = oi.Price
                    }).ToList()
                });

            var totalOrders = await _orderRepository.Table.CountAsync(o => o.UserId == userId);

            var orders = await query.ToListAsync();

            return new UserOrdersViewModel
            {
                Orders = orders,
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
