using MexicanRestaurant.Core.Extensions;
using MexicanRestaurant.Core.Interfaces;
using MexicanRestaurant.Core.Models;
using MexicanRestaurant.Core.Specifications;
using MexicanRestaurant.WebUI.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MexicanRestaurant.Core.Services
{
    public class OrderService : IOrderService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRepository<Product> _products;
        private readonly IRepository<Order> _orders;

        public OrderService(IHttpContextAccessor httpContextAccessor, IRepository<Product> products, IRepository<Order> orders)
        {
            _httpContextAccessor = httpContextAccessor;
            _products = products;
            _orders = orders;
        }

        public OrderViewModel GetCurrentOrderFromSession()
        {
            return _httpContextAccessor.HttpContext.Session.Get<OrderViewModel>("OrderViewModel");
        }

        public void SaveCurrentOrderToSession(OrderViewModel model)
        {
            model.TotalAmount = model.OrderItems.Sum(i => i.Price * i.Quantity);
            _httpContextAccessor.HttpContext.Session.Set("OrderViewModel", model);
        }

        public async Task<OrderViewModel> InitializeOrderViewModelAsync(int pageNumber = 1, int pageSize = 6)
        {
            var options = new QueryOptions<Product>
            {
                OrderBy = p => p.Name,
            };

            var allProducts = await _products.GetAllAsync(options);
            var totalProducts = allProducts.Count();
            var products = allProducts.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            return new OrderViewModel
            {
                OrderItems = new List<OrderItemViewModel>(),
                Products = products.ToList(),
                CurrentPage = pageNumber,
                TotalPages = (int)Math.Ceiling((double)totalProducts / pageSize),
            };
        }

        public async Task AddItemToOrderAsync(int productId, int productQantity)
        {
            
            var product = await _products.GetByIdAsync(productId, new QueryOptions<Product>());
            if (product == null || productQantity <= 0) return;

            var model = GetCurrentOrderFromSession() ?? await InitializeOrderViewModelAsync();

            var existingItem = model.OrderItems.FirstOrDefault(i => i.ProductId == productId);

            if (existingItem != null)
                existingItem.Quantity += productQantity;

            else
            {
                model.OrderItems.Add(new OrderItemViewModel
                {
                    ProductId = productId,
                    ProductName = product.Name,
                    ImageUrl = product.ImageUrl,
                    Quantity = productQantity,
                    Price = product.Price
                });
            }
            SaveCurrentOrderToSession(model);
        }

        public async Task PlaceOrderAsync(string userId)
        {
            var model = GetCurrentOrderFromSession();
            if (model == null || model.OrderItems.Count == 0) return;

            Order order = new Order
            {
                UserId = userId,
                TotalAmount = model.TotalAmount,
                OrderDate = DateTime.Now,
                OrderItems = model.OrderItems.Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    Price = i.Price
                }).ToList()
            };
            await _orders.AddAsync(order);
            _httpContextAccessor.HttpContext.Session.Remove("OrderViewModel");
        }

        public async Task<List<Order>> GetUserOrdersAsync(string userId)
        {
            return (await _orders.GetAllByIdAsync(userId, "UserId", new QueryOptions<Order>
            {
                Includes = "OrderItems.Product",
            })).ToList();
        }

        public void IncreaseItemQuantity(int productId)
        {
            var model = GetCurrentOrderFromSession();
            if (model == null) return;
            var item = model.OrderItems.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                item.Quantity++;
                SaveCurrentOrderToSession(model);
            }
        }

        public void DecreaseItemQuantity(int productId)
        {
            var model = GetCurrentOrderFromSession();
            if (model == null) return;
            var item = model.OrderItems.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                if (item.Quantity > 1)
                {
                    item.Quantity--;
                    SaveCurrentOrderToSession(model);
                }
                else
                {
                    model.OrderItems.Remove(item);
                    SaveCurrentOrderToSession(model);
                }
            }
        }
        public void RemoveItemFromOrder(int productId)
        {
            var model = GetCurrentOrderFromSession();
            if (model == null) return;
            var item = model.OrderItems.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                model.OrderItems.Remove(item);
                SaveCurrentOrderToSession(model);
            }
        }
    }
}
