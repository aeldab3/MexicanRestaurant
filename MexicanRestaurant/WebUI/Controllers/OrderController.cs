using MexicanRestaurant.Application.Services;
using MexicanRestaurant.Core.Extensions;
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
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService, UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
            _orderService = orderService;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Create(int page = 1, string searchTerm = "", int? categoryId = null, string sortBy = "")
        {
            var sessionModel = _orderService.GetCurrentOrderFromSession();
            var newModel = await _orderService.InitializeOrderViewModelAsync(
                new FilterOptionsViewModel { SearchTerm = searchTerm, SelectedCategoryId = categoryId, SortBy = sortBy },
                new PaginationInfo { CurrentPage = page, PageSize = 8 });

            if (sessionModel != null)
            {
                newModel.OrderItems = sessionModel.OrderItems;
                newModel.TotalAmount = sessionModel.TotalAmount;
            }
            _orderService.SaveCurrentOrderToSession(newModel);
            return View(newModel);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddItem(int prodId, int prodQty, int page = 1)
        {
            await _orderService.AddItemToOrderAsync(prodId, prodQty);
            var model = _orderService.GetCurrentOrderFromSession();
            var totalQuantity = model.OrderItems.Sum(item => item.Quantity);
            return Json(new { success = true, totalQuantity });
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Cart()
        {
            var model = _orderService.GetCurrentOrderFromSession();
            if (model == null || model.OrderItems.Count == 0)
                return RedirectToAction("Create");
            return View(model);
        }

        [HttpGet]
        [Authorize]
        public IActionResult CartPartial()
        {
            var model = _orderService.GetCurrentOrderFromSession();
            return PartialView("_CartPartial", model);
        }

        [HttpGet]
        [Authorize]
        public IActionResult CartIconPartial()
        {
            var model = _orderService.GetCurrentOrderFromSession();
            return PartialView("_CartIconPartial", model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder()
        {
            await _orderService.PlaceOrderAsync(_userManager.GetUserId(User));
            return RedirectToAction("ViewOrders");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ViewOrders()
        {
            var orders = await _orderService.GetUserOrdersAsync(_userManager.GetUserId(User));
            return View(orders);
        }

        [HttpPost]
        public async Task<IActionResult> Increase(int productId)
        {
            await _orderService.IncreaseItemQuantity(productId);
            return RedirectToAction("Cart");
        }

        [HttpPost]
        public async Task<IActionResult> Decrease(int productId)
        {
            await _orderService.DecreaseItemQuantity(productId);
            return RedirectToAction("Cart");
        }

        [HttpPost]
        public async Task<IActionResult> Remove(int productId)
        {
            await _orderService.RemoveItemFromOrder(productId);
            return RedirectToAction("Cart");
        }
    }
}
