using AutoMapper;
using MexicanRestaurant.Core.Enums;
using MexicanRestaurant.Core.Interfaces;
using MexicanRestaurant.Core.Models;
using MexicanRestaurant.Core.Specifications;
using MexicanRestaurant.WebUI.ViewModels;

namespace MexicanRestaurant.Application.Services
{
    public class CheckoutService : ICheckoutService
    {
        private readonly IOrderCartService _orderCartService;
        private readonly IRepository<DeliveryMethod> _deliveryRepo;
        private readonly IRepository<Order> _orderRepo;
        private readonly IMapper _mapper;

        public CheckoutService(IOrderCartService orderCartService, IRepository<DeliveryMethod> deliveryRepo, IRepository<Order> orderRepo, IMapper mapper)
        {
            _orderCartService = orderCartService;
            _deliveryRepo = deliveryRepo;
            _orderRepo = orderRepo;
            _mapper = mapper;
        }

        public async Task<CheckoutViewModel> PrepareCheckoutAsync(string userId)
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
                TotalAmount = cart.TotalAmount,
                SelectedPaymentMethod = "Cash"
            };
        }

        public async Task ProcessCheckoutAsync(string userId, CheckoutViewModel checkoutVM)
        {
            var cart = _orderCartService.GetCurrentOrderFromSession();
            if (cart == null || !cart.OrderItems.Any())
                throw new InvalidOperationException("Cart is empty");

            var selectedDeliveryMethod = await _deliveryRepo.GetByIdAsync(checkoutVM.SelectedDeliveryMethodId, new QueryOptions<DeliveryMethod>())
                                        ?? throw new InvalidOperationException("Invalid delivery method");

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
                PaymentMethod = checkoutVM.SelectedPaymentMethod
            };
            await _orderRepo.AddAsync(newOrder);
            _orderCartService.SaveCurrentOrderToSession(null);
        }
    }
}
