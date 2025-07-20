using AutoMapper;
using MexicanRestaurant.Application.Helpers;
using MexicanRestaurant.Core.Interfaces;
using MexicanRestaurant.Core.Models;
using MexicanRestaurant.Core.Specifications;
using MexicanRestaurant.Views.Shared;
using MexicanRestaurant.WebUI.ViewModels;

namespace MexicanRestaurant.Application.Services
{
    public class OrderCartService : IOrderCartService
    {
        private readonly ISessionService _sessionService;
        private readonly IOrderViewModelFactory _orderViewModelFactory;
        private readonly IRepository<Product> _products;
        private readonly IMapper _mapper;

        private const string OrderSessionCartKey = "OrderViewModel";
        public OrderCartService(ISessionService sessionService, IOrderViewModelFactory orderViewModelFactory, IRepository<Product> products, IMapper mapper)
        {
            _sessionService = sessionService;
            _orderViewModelFactory = orderViewModelFactory;
            _products = products;
            _mapper = mapper;
        }

        public OrderViewModel GetCurrentOrderFromSession() =>
             _sessionService.Get<OrderViewModel>(OrderSessionCartKey);

        public void SaveCurrentOrderToSession(OrderViewModel model)
        {
            if (model == null)
            {
                _sessionService.Remove(OrderSessionCartKey);
                return;
            }
            model.TotalAmount = model.OrderItems.Sum(i => i.Price * i.Quantity);
            _sessionService.Set(OrderSessionCartKey, model);
        }

        public async Task AddToOrderAsync(int productId, int productQantity)
        {
            var product = await _products.GetByIdAsync(productId, new QueryOptions<Product>());
            if (product == null || productQantity <= 0 || product.Stock < productQantity) throw new ProductNotFoundException("Cannot found This product or Invalid quantity");

            var currentPage = GetCurrentOrderFromSession()?.Pagination.CurrentPage ?? 1;
            var model = GetCurrentOrderFromSession() ?? await _orderViewModelFactory.InitializeOrderViewModelAsync(
                new FilterOptionsViewModel { SearchTerm = "", SelectedCategoryId = null, SortBy = "" },
                new PaginationInfo { CurrentPage = currentPage, PageSize = 8 }
            );
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

        public async Task IncreaseQuantityAsync(int productId)
        {
            var model =  GetCurrentOrderFromSession();
            if (model == null) return;
            var item = model.OrderItems.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                var product = await _products.GetByIdAsync(productId, new QueryOptions<Product>());
                if (product == null || product.Stock <= 0) throw new ProductNotFoundException("Cannot found This product or Invalid quantity");
                item.Quantity++;
                product.Stock--;
                await _products.UpdateAsync(product);
                SaveCurrentOrderToSession(model);
            }
        }

        public async Task DecreaseQuantityAsync(int productId)
        {
            var model = GetCurrentOrderFromSession();
            if (model == null) return;
            var item = model.OrderItems.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                var product = await _products.GetByIdAsync(productId, new QueryOptions<Product>());
                if (product == null) throw new ProductNotFoundException("Cannot found This product");

                product.Stock++;

                if (item.Quantity > 1)
                    item.Quantity--;
                else
                    model.OrderItems.Remove(item);

                await _products.UpdateAsync(product);
                SaveCurrentOrderToSession(model);
            }
        }

        public async Task RemoveFromOrderAsync(int productId)
        {
            var model = GetCurrentOrderFromSession();
            if (model == null) return;
            var item = model.OrderItems.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                var product = await _products.GetByIdAsync(productId, new QueryOptions<Product>());
                if (product == null) throw new ProductNotFoundException("Cannot found This product");

                product.Stock += item.Quantity;
                await _products.UpdateAsync(product);
                model.OrderItems.Remove(item);
                SaveCurrentOrderToSession(model);
            }
        }
    }
}
