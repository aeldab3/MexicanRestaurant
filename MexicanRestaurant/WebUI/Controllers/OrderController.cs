using MexicanRestaurant.Core.Interfaces;
using MexicanRestaurant.Core.Models;
using MexicanRestaurant.Views.Shared;
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

        public OrderController(IOrderCartService orderCartService, UserManager<ApplicationUser> userManager, IOrderViewModelFactory orderViewModelFactory, IOrderProcessor orderProcessor)
        {
            _userManager = userManager;
            _orderCartService = orderCartService;
            _orderViewModelFactory = orderViewModelFactory;
            _orderProcessor = orderProcessor;
        }

        [HttpGet]
        [Authorize]
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
            return Json(new { success = true, totalQuantity });
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
            await _orderProcessor.PlaceOrderAsync(_userManager.GetUserId(User));
            return RedirectToAction("ViewOrders");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ViewOrders()
        {
            var orders = await _orderProcessor.GetUserOrdersAsync(_userManager.GetUserId(User));
            return View(orders);
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
