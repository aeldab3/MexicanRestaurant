using AutoMapper;
using AutoMapper.QueryableExtensions;
using MexicanRestaurant.Core.Interfaces;
using MexicanRestaurant.Core.Models;
using MexicanRestaurant.Views.Shared;
using MexicanRestaurant.WebUI.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace MexicanRestaurant.Application.Services
{
    public class OrderProcessor : IOrderProcessor
    {
        private readonly IRepository<Order> _orderRepository;
        private readonly IMapper _mapper;

        public OrderProcessor(IRepository<Order> orderRepository, IMapper mapper)
        {
            _orderRepository = orderRepository;
            _mapper = mapper;
        }

        public async Task<UserOrdersViewModel> GetPagedUserOrdersAsync(string userId, PaginationInfo pagination)
        {
            var query = _orderRepository.Table
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .Skip((pagination.CurrentPage - 1) * pagination.PageSize)
                .Take(pagination.PageSize);

            var totalOrders = await _orderRepository.Table.CountAsync(o => o.UserId == userId);

            var orders = await query.ProjectTo<OrderListItemViewModel>(_mapper.ConfigurationProvider).ToListAsync();

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
