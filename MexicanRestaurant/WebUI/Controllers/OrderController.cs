using MexicanRestaurant.Application.Services;
using MexicanRestaurant.Core.Interfaces;
using MexicanRestaurant.Core.Models;
using MexicanRestaurant.Views.Shared;
using MexicanRestaurant.WebUI.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MexicanRestaurant.WebUI.Controllers
{
    public class OrderController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IOrderCartService _orderCartService;
        private readonly IOrderViewModelFactory _orderViewModelFactory;
        private readonly IOrderProcessor _orderProcessor;
        private readonly ICheckoutService _checkoutService;
        private readonly ISharedLookupService _sharedLookupService;
        private readonly PaymentStrategyResolver _paymentStrategyResolver;

        public OrderController(IOrderCartService orderCartService, UserManager<ApplicationUser> userManager, IOrderViewModelFactory orderViewModelFactory, IOrderProcessor orderProcessor, ICheckoutService checkoutService, ISharedLookupService sharedLookupService, PaymentStrategyResolver paymentStrategyResolver)
        {
            _userManager = userManager;
            _orderCartService = orderCartService;
            _orderViewModelFactory = orderViewModelFactory;
            _orderProcessor = orderProcessor;
            _checkoutService = checkoutService;
            _sharedLookupService = sharedLookupService;
            _paymentStrategyResolver = paymentStrategyResolver;
        }

        [HttpGet]
        public async Task<IActionResult> Create(int page = 1, string searchTerm = "", int? categoryId = null, string sortBy = "")
        {
            var model = _orderCartService.GetCurrentOrderFromSession();
            var newModel = await _orderViewModelFactory.InitializeOrderViewModelAsync(
                new FilterOptionsViewModel { SearchTerm = searchTerm, SelectedCategoryId = categoryId, SortBy = sortBy },
                new PaginationInfo { CurrentPage = page, PageSize = 8 });

            if (model != null)
            {
                newModel.OrderItems = model.OrderItems;
                newModel.TotalAmount = model.TotalAmount;
            }
            _orderCartService.SaveCurrentOrderToSession(newModel);
            return View(newModel);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddItem(int prodId, int prodQty, int page = 1)
        {
            await _orderCartService.AddToOrderAsync(prodId, prodQty);
            var model = _orderCartService.GetCurrentOrderFromSession();
            var totalQuantity = model.OrderItems.Sum(item => item.Quantity);
            if (totalQuantity == 0)
            {
                TempData["Error"] = "Your cart is empty. Please add items before proceeding to checkout.";
                return RedirectToAction("Create");
            }
            TempData["Success"] = "Item added to cart successfully.";
            return Json(new { success = true, totalQuantity });
        }

        [HttpGet]
        [Authorize]
         public IActionResult Cart()
         {
            var model = _orderCartService.GetCurrentOrderFromSession();
            if (model == null || model.OrderItems.Count == 0)
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("Create");
            }
            return View(model);
         }

        [HttpGet]
        [Authorize]
        public IActionResult CartPartial()
        {
            var model = _orderCartService.GetCurrentOrderFromSession();
            return PartialView("_CartPartial", model);
        }

        [HttpGet]
        [Authorize]
        public IActionResult CartIconPartial()
        {
            var model = _orderCartService.GetCurrentOrderFromSession();
            return PartialView("_CartIconPartial", model);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var userId = _userManager.GetUserId(User);
            if (userId is null)
            {
                TempData["Error"] = "You must be logged in to complete the checkout process.";
                return RedirectToAction("Login", "Account");
            }
            var checkoutVM = await _checkoutService.PrepareCheckoutAsync(userId);
            checkoutVM.StripePublishableKey = Environment.GetEnvironmentVariable("StripePublishableKey") ?? throw new InvalidOperationException("Stripe publishable key is missing");
            return View(checkoutVM);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutViewModel checkoutVM)
        {
            if (!ModelState.IsValid)
            {
                var cart = _orderCartService.GetCurrentOrderFromSession();
                checkoutVM.ShippingAddress ??= new ShippingAddressViewModel();
                checkoutVM.AvailableDeliveryMethods = await _sharedLookupService.GetAllDeliveryMethodsAsync();
                checkoutVM.OrderItems = cart?.OrderItems ?? new List<OrderItemViewModel>();
                checkoutVM.TotalAmount = cart?.TotalAmount ?? 0;
                ViewBag.Step = "1";
                TempData["Error"] = "Please correct the errors in the form.";
                return View(checkoutVM);
            }

            var userId = _userManager.GetUserId(User);
            if (userId is null)
            {
                TempData["Error"] = "You must be logged in to complete the checkout process.";
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var orderId = await _checkoutService.ProcessCheckoutAsync(userId, checkoutVM);
                if (checkoutVM.SelectedPaymentMethod == "stripe")
                {
                    var paymentRequest = new PaymentRequest
                    {
                        Amount = checkoutVM.TotalAmount,
                        Currency = "usd",
                        Description = $"Order #{orderId} payment for user {userId}",
                        OrderId = orderId
                    };
                    var paymentStrategy = _paymentStrategyResolver.Resolve(checkoutVM.SelectedPaymentMethod);
                    var paymentResult = await paymentStrategy.ProcessPaymentAsync(paymentRequest);
                    if (!paymentResult.IsSuccess)
                        return Json(new { IsSuccess = false, message = paymentResult.Message });

                    return Json(new { IsSuccess = true, clientSecret = paymentResult.Message });
                }
                TempData["Success"] = "Your order has been done successfully.";
                return RedirectToAction("ViewOrders");
            }
            catch (Exception ex)
            {
                return Json(new { IsSuccess = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Authorize]
        public IActionResult CheckoutSuccess()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        public IActionResult CheckoutCancel()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ViewOrders(int page = 1)
        {
            var userId = _userManager.GetUserId(User);
            var pagination = new PaginationInfo { CurrentPage = page, PageSize = 15 };
            if (userId is null)
            {
                TempData["Error"] = "You must be logged in to view your orders.";
                return RedirectToAction("Login", "Account");
            }
            var model = await _orderProcessor.GetPagedUserOrdersAsync(userId, pagination);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Increase(int productId)
        {
            await _orderCartService.IncreaseQuantityAsync(productId);
            return RedirectToAction("Cart");
        }

        [HttpPost]
        public async Task<IActionResult> Decrease(int productId)
        {
            await _orderCartService.DecreaseQuantityAsync(productId);
            return RedirectToAction("Cart");
        }

        [HttpPost]
        public async Task<IActionResult> Remove(int productId)
        {
            await _orderCartService.RemoveFromOrderAsync(productId);
            return RedirectToAction("Cart");
        }
    }
}
