using AutoMapper;
using MexicanRestaurant.Application.Helpers;
using MexicanRestaurant.Core.Extensions;
using MexicanRestaurant.Core.Interfaces;
using MexicanRestaurant.Core.Models;
using MexicanRestaurant.Core.Specifications;
using MexicanRestaurant.Views.Shared;
using MexicanRestaurant.WebUI.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace MexicanRestaurant.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly ISessionService _sessionService;
        private readonly IRepository<Product> _products;
        private readonly IRepository<Order> _orders;
        private readonly IRepository<Category> _categories;
        private readonly IMapper _mapper;

        public OrderService(ISessionService sessionService, IRepository<Product> products, IRepository<Order> orders, IRepository<Category> categories, IMapper mapper)
        {
            _sessionService = sessionService;
            _products = products;
            _orders = orders;
            _categories = categories;
            _mapper = mapper;
        }

        public OrderViewModel GetCurrentOrderFromSession()
        {
            return _sessionService.Get<OrderViewModel>("OrderViewModel");
        }

        public void SaveCurrentOrderToSession(OrderViewModel model)
        {
            model.TotalAmount = model.OrderItems.Sum(i => i.Price * i.Quantity);
            _sessionService.Set("OrderViewModel", model);
        }

        public async Task<OrderViewModel> InitializeOrderViewModelAsync(FilterOptionsViewModel filter, PaginationInfo pagination)
        {
            var options = new QueryOptions<Product>
            {
                Includes = nameof(Product.Category),
                PageNumber = pagination.CurrentPage,
                PageSize = pagination.PageSize,
                Where = ProductFilteringHelper.BuildFilter(filter.SearchTerm, filter.SelectedCategoryId),
                OrderByWithFunc = ProductFilteringHelper.BuildOrderBy(filter.SortBy)
            };
            var allProducts = await _products.GetAllAsync(options);
            var mappedProducts = _mapper.Map<List<ProductViewModel>>(allProducts.ToList());
            var countOptions = new QueryOptions<Product>
            {
                Where = options.Where,
                Includes = nameof(Product.Category),
                DisablePaging = true
            };
            var totalProducts = (await _products.GetAllAsync(countOptions)).Count();
            var categories = await GetCategorySelectListAsync();
            return new OrderViewModel
            {
                Products = mappedProducts,
                Filter = new FilterOptionsViewModel
                {
                    SearchTerm = filter.SearchTerm,
                    SelectedCategoryId = filter.SelectedCategoryId,
                    SortBy = filter.SortBy,
                    Categories = categories
                },
                Pagination = new PaginationInfo
                {
                    CurrentPage = pagination.CurrentPage,
                    PageSize = pagination.PageSize,
                    TotalPages = (int)Math.Ceiling((double)totalProducts / pagination.PageSize)
                },
                OrderItems = GetCurrentOrderFromSession()?.OrderItems ?? new List<OrderItemViewModel>(),
            };
        }

        public async Task AddItemToOrderAsync(int productId, int productQantity)
        {
            var product = await _products.GetByIdAsync(productId, new QueryOptions<Product>());
            if (product == null || productQantity <= 0 || product.Stock < productQantity) return;

            var currentPage = GetCurrentOrderFromSession()?.Pagination.CurrentPage ?? 1;
            var model = GetCurrentOrderFromSession() ?? await InitializeOrderViewModelAsync(
                new FilterOptionsViewModel { SearchTerm = "", SelectedCategoryId = null, SortBy = ""}, 
                new PaginationInfo { CurrentPage = currentPage, PageSize = 8 });
            var existingItem = model.OrderItems.FirstOrDefault(i => i.ProductId == productId);

            if (existingItem != null)
                existingItem.Quantity += productQantity;

            else
            {
                var orderItem = _mapper.Map<OrderItemViewModel>(product);
                orderItem.Quantity = productQantity;
                model.OrderItems.Add(orderItem);
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
                OrderItems = _mapper.Map<List<OrderItem>>(model.OrderItems)
            };
            await _orders.AddAsync(order);
            _sessionService.Remove("OrderViewModel");
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
