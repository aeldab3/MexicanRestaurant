using AutoMapper;
using MexicanRestaurant.Application.Helpers;
using MexicanRestaurant.Core.Enums;
using MexicanRestaurant.Core.Interfaces;
using MexicanRestaurant.Core.Models;
using MexicanRestaurant.Core.Specifications;
using MexicanRestaurant.WebUI.ViewModels;
using Newtonsoft.Json;

namespace MexicanRestaurant.Application.Services
{
    public class CheckoutService : ICheckoutService
    {
        private readonly IOrderCartService _orderCartService;
        private readonly IRepository<DeliveryMethod> _deliveryRepo;
        private readonly IRepository<Order> _orderRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<CheckoutService> _logger;

        public CheckoutService(IOrderCartService orderCartService, IRepository<DeliveryMethod> deliveryRepo, IRepository<Order> orderRepo, IMapper mapper, ILogger<CheckoutService> logger)
        {
            _orderCartService = orderCartService;
            _deliveryRepo = deliveryRepo;
            _orderRepo = orderRepo;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<CheckoutViewModel> PrepareCheckoutAsync(string userId)
        {
            try
            {
                var cart = _orderCartService.GetCurrentOrderFromSession();
                if (cart == null || !cart.OrderItems.Any())
                    throw new InvalidOperationException("Cart is empty");

                var deliveryMethods = await _deliveryRepo.GetAllAsync();

                return new CheckoutViewModel
                {
                    ShippingAddress = new ShippingAddressViewModel(),
                    AvailableDeliveryMethods = deliveryMethods.ToList(),
                    SelectedDeliveryMethodId = 0,
                    OrderItems = cart.OrderItems,
                    TotalAmount = cart.TotalAmount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Prepare Checkout");
                throw new ProductNotFoundException("An error occurred while Prepare Checkout");
            }
        }

        public async Task ProcessCheckoutAsync(string userId, CheckoutViewModel checkoutVM)
        {
            try
            {
                var cart = _orderCartService.GetCurrentOrderFromSession();
                if (cart == null || !cart.OrderItems.Any())
                    throw new InvalidOperationException("Cart is empty");

                var selectedDeliveryMethod = await _deliveryRepo.GetByIdAsync(checkoutVM.SelectedDeliveryMethodId, new QueryOptions<DeliveryMethod>());
                if (selectedDeliveryMethod == null)
                    throw new InvalidOperationException("Invalid delivery method");

                var mappedItems = _mapper.Map<List<OrderItem>>(cart.OrderItems);
                var newOrder = new Order
                {
                    UserId = userId,
                    OrderDate = DateTime.Now,
                    TotalAmount = cart.TotalAmount + selectedDeliveryMethod.Price,
                    DeliveryMethodId = selectedDeliveryMethod.Id,
                    DeliveryMethod = selectedDeliveryMethod,
                    Status = OrderStatus.Pending,
                    ShippingAddress = new Address
                    {
                        FirstName = checkoutVM.ShippingAddress.FirstName,
                        LastName = checkoutVM.ShippingAddress.LastName,
                        Street = checkoutVM.ShippingAddress.Street,
                        City = checkoutVM.ShippingAddress.City,
                        State = checkoutVM.ShippingAddress.State,
                        Zipcode = checkoutVM.ShippingAddress.Zipcode,
                        PhoneNumber = checkoutVM.ShippingAddress.PhoneNumber
                    },

                    OrderItems = mappedItems,

                };
                await _orderRepo.AddAsync(newOrder);
                _orderCartService.SaveCurrentOrderToSession(null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error placing order");
                throw new ProductNotFoundException("An error occurred while placing the order");
            }
        }
    }
}
