using MexicanRestaurant.Application.Helpers;
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
        private readonly ILogger<OrderController> _logger;

        public OrderController(IOrderCartService orderCartService, UserManager<ApplicationUser> userManager, IOrderViewModelFactory orderViewModelFactory, IOrderProcessor orderProcessor, ILogger<OrderController> logger)
        {
            _userManager = userManager;
            _orderCartService = orderCartService;
            _orderViewModelFactory = orderViewModelFactory;
            _orderProcessor = orderProcessor;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Create(int page = 1, string searchTerm = "", int? categoryId = null, string sortBy = "")
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order view model");
                TempData["ErrorMessage"] = "An error occurred while creating the order view model.";
                return RedirectToAction("Create", "Order");
            }
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddItem(int prodId, int prodQty, int page = 1)
        {
            try { 
                await _orderCartService.AddToOrderAsync(prodId, prodQty);
                var model = _orderCartService.GetCurrentOrderFromSession();
                var totalQuantity = model.OrderItems.Sum(item => item.Quantity);
                return Json(new { success = true, totalQuantity });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding item to order");
                return Json(new { success = false, message = "An error occurred while adding the item to the order." });
            }
        }

        [HttpGet]
        [Authorize]
         public IActionResult Cart()
         {
            var model = _orderCartService.GetCurrentOrderFromSession();
            if (model == null || model.OrderItems.Count == 0)
                return RedirectToAction("Create");
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

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder()
        {
            try 
            { 
                await _orderProcessor.PlaceOrderAsync(_userManager.GetUserId(User));
                return RedirectToAction("ViewOrders");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error placing order");
                TempData["ErrorMessage"] = "An error occurred while placing the order.";
                return RedirectToAction("Cart");
            }
        }


        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ViewOrders(int page = 1)
        {
            var userId = _userManager.GetUserId(User);
            var pagination = new PaginationInfo { CurrentPage = page, PageSize = 15 };
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
