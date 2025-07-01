using MexicanRestaurant.Core.Extensions;
using MexicanRestaurant.Core.Interfaces;
using MexicanRestaurant.Core.Models;
using MexicanRestaurant.Core.Specifications;
using MexicanRestaurant.WebUI.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace MexicanRestaurant.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRepository<Product> _products;
        private readonly IRepository<Order> _orders;
        private readonly IRepository<Category> _categories;

        public OrderService(IHttpContextAccessor httpContextAccessor, IRepository<Product> products, IRepository<Order> orders, IRepository<Category> categories)
        {
            _httpContextAccessor = httpContextAccessor;
            _products = products;
            _orders = orders;
            _categories = categories;
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

        public async Task<OrderViewModel> InitializeOrderViewModelAsync(int pageNumber, int pageSize, string searchTerm, int? categoryId, string sortBy)
        {
            var options = new QueryOptions<Product>
            {
                Includes = nameof(Product.Category),
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            Expression<Func<Product, bool>> filter = p => true;

            if (!string.IsNullOrEmpty(searchTerm))
                filter = filter.AndAlso(p => p.Name.Contains(searchTerm.ToLower()) || p.Description.Contains(searchTerm.ToLower()));

            if (categoryId.HasValue && categoryId.Value > 0)
                filter = filter.AndAlso(p => p.CategoryId == categoryId.Value);

            if (filter != null)
                options.Where = filter;

            if (!string.IsNullOrEmpty(sortBy))
            {
                options.OrderBy = sortBy switch
                {
                    "name_asc" => p => p.Name,
                    "name_desc" => p => p.Name,
                    "price_asc" => p => p.Price,
                    "price_desc" => p => p.Price,
                    _ => p => p.Name
                };
                if (sortBy == "name_desc" || sortBy == "price_desc")
                    options.IsDescending = true;
            }
            else
                options.OrderBy = p => p.Name;

            var allProducts = await _products.GetAllAsync(options);
            var countOptions = new QueryOptions<Product>
            {
                Where = options.Where,
                Includes = nameof(Product.Category),
                PageNumber = 0,
                PageSize = 0
            };
            var totalProducts = (await _products.GetAllAsync(countOptions)).Count();
            var categories = await GetCategorySelectListAsync();
            return new OrderViewModel
            {
                Products = allProducts.ToList(),
                CurrentPage = pageNumber,
                TotalPages = (int)Math.Ceiling((double)totalProducts / pageSize),
                SearchTerm = searchTerm,
                SelectedCategoryId = categoryId,
                SortBy = sortBy,
                Categories = categories,
                OrderItems = GetCurrentOrderFromSession()?.OrderItems ?? new List<OrderItemViewModel>(),
            };
        }

        public async Task AddItemToOrderAsync(int productId, int productQantity)
        {
            var product = await _products.GetByIdAsync(productId, new QueryOptions<Product>());
            if (product == null || productQantity <= 0 || product.Stock < productQantity) return;

            var currentPage = GetCurrentOrderFromSession()?.CurrentPage ?? 1;
            var model = GetCurrentOrderFromSession() ?? await InitializeOrderViewModelAsync(currentPage, 8, null, null, null);
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
            product.Stock -= productQantity;
            await _products.UpdateAsync(product);
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
                var product = _products.GetByIdAsync(productId, new QueryOptions<Product>()).Result;
                if (product == null || product.Stock <= 0) return;
                item.Quantity++;
                product.Stock--;
                _products.UpdateAsync(product).Wait();
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
                var product = _products.GetByIdAsync(productId, new QueryOptions<Product>()).Result;
                if (product == null) return;

                product.Stock++;

                if (item.Quantity > 1)
                {
                    item.Quantity--;
                    _products.UpdateAsync(product).Wait();
                }
                else
                {
                    model.OrderItems.Remove(item);
                    _products.UpdateAsync(product).Wait();
                }
                    SaveCurrentOrderToSession(model);
            }
        }
        public void RemoveItemFromOrder(int productId)
        {
            var model = GetCurrentOrderFromSession();
            if (model == null) return;
            var item = model.OrderItems.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                var product = _products.GetByIdAsync(productId, new QueryOptions<Product>()).Result;
                if (product != null)
                {
                    product.Stock += item.Quantity;
                    _products.UpdateAsync(product).Wait();
                }
                model.OrderItems.Remove(item);
                SaveCurrentOrderToSession(model);
            }
        }
        public async Task<List<SelectListItem>> GetCategorySelectListAsync()
        {
            var categories = await _categories.GetAllAsync();
            return categories.Select(c => new SelectListItem
            {
                Value = c.CategoryId.ToString(),
                Text = c.Name
            }).ToList();
        }

    }
}
