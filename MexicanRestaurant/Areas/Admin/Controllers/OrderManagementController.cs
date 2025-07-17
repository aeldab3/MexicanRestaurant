using MexicanRestaurant.Core.Enums;
using MexicanRestaurant.Core.Interfaces;
using MexicanRestaurant.Core.Models;
using MexicanRestaurant.Core.Specifications;
using MexicanRestaurant.Views.Shared;
using MexicanRestaurant.WebUI.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MexicanRestaurant.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin, Manager")]
    public class OrderManagementController : Controller
    {
        private readonly IRepository<Order> _orderRepository;
        private readonly ILogger<OrderManagementController> _logger;
        public OrderManagementController(IRepository<Order> orderRepository, ILogger<OrderManagementController> logger)
        {
            _orderRepository = orderRepository;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            try
            {
                var pagination = new PaginationInfo { CurrentPage = page, PageSize = 15 };
                var orders = await _orderRepository.Table
                    .AsNoTracking()
                    .OrderByDescending(o => o.OrderDate)
                    .Skip((pagination.CurrentPage - 1) * pagination.PageSize)
                    .Take(pagination.PageSize)
                    .Select(o => new OrderListItemViewModel
                    {
                        OrderId = o.OrderId,
                        OrderDate = o.OrderDate,
                        TotalAmount = o.TotalAmount,
                        Status = o.Status,
                        DeliveryShortName = o.DeliveryMethod.ShortName ?? string.Empty,
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
                        }).ToList(),
                        UserFullName = o.User.FirstName + " " + o.User.LastName,
                        UserEmail = o.User.Email
                    }).ToListAsync();

                var totalOrders = await _orderRepository.Table.CountAsync();

                var viewModel = new UserOrdersViewModel
                {
                    Orders = orders.ToList(),
                    Pagination = new PaginationInfo
                    {
                        CurrentPage = pagination.CurrentPage,
                        PageSize = pagination.PageSize,
                        TotalPages = (int)Math.Ceiling((double)totalOrders / pagination.PageSize)
                    }
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders");
                return View("Error", new ErrorViewModel { Message = "An error occurred while retrieving orders." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int orderId, OrderStatus newStatus, int currentPage = 1)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                var order = await _orderRepository.GetByIdAsync(orderId, new QueryOptions<Order>());
                if (order == null)
                    return NotFound();

                order.Status = newStatus;
                TempData["Success"] = "Order status updated successfully.";
                await _orderRepository.UpdateAsync(order);
                return RedirectToAction("Index", new { page = currentPage });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status");
                TempData["ErrorMessage"] = "An error occurred while loading the dashboard. Please try again later.";
                return View("Error", new { message = "An error occurred while updating the order status." });
            }
        }

    }
}
