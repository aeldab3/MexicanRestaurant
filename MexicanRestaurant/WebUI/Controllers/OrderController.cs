using MexicanRestaurant.Core.Extensions;
using MexicanRestaurant.Core.Interfaces;
using MexicanRestaurant.Core.Models;
using MexicanRestaurant.Core.Services;
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
        public async Task<IActionResult> Create()
        {
            var model = _orderService.GetCurrentOrderFromSession() ?? await _orderService.InitializeOrderViewModelAsync();
            return View(model);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddItem(int prodId, int prodQty)
        {
            await _orderService.AddItemToOrderAsync(prodId, prodQty);
            return RedirectToAction("Create");
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

        [HttpPost]
        [Authorize]
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
        public IActionResult Increase(int productId)
        {
            _orderService.IncreaseItemQuantity(productId);
            return RedirectToAction("Cart");
        }

        [HttpPost]
        public IActionResult Decrease(int productId)
        {
           
            _orderService.DecreaseItemQuantity(productId);
            return RedirectToAction("Cart");
        }
    }    
}
