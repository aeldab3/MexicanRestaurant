using MexicanRestaurant.Core.Extensions;
using MexicanRestaurant.Core.Interfaces;
using MexicanRestaurant.Core.Models;
using MexicanRestaurant.Core.Specifications;
using MexicanRestaurant.Infrastructure.Data;
using MexicanRestaurant.WebUI.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MexicanRestaurant.WebUI.Controllers
{
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private IRepository<Product> _products;
        private IRepository<Order> _orders;

        public OrderController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IRepository<Product> products, IRepository<Order> orders)
        {
            _context = context;
            _userManager = userManager;
            _products = products;
            _orders = orders;
        }
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Create()
        {
            var model = HttpContext.Session.Get<OrderViewModel>("OrderViewModel") ?? new OrderViewModel
            {
                OrderItems = new List<OrderItemViewModel>(),
                Products = await _products.GetAllAsync(),
            };
            return View(model);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddItem(int prodId, int prodQty)
        {
            var product = await _context.Products.FindAsync(prodId);
            if (product == null || prodQty <= 0)
                return NotFound();

            var model = HttpContext.Session.Get<OrderViewModel>("OrderViewModel") ?? new OrderViewModel
            {
                OrderItems = new List<OrderItemViewModel>(),
                Products = await _products.GetAllAsync(),
            };

            var existingItem = model.OrderItems.FirstOrDefault(i => i.ProductId == prodId);

            if (existingItem != null)
                existingItem.Quantity += prodQty;

            else
            {
                model.OrderItems.Add(new OrderItemViewModel
                {
                    ProductId = prodId,
                    ProductName = product.Name,
                    ImageUrl = product.ImageUrl,
                    Quantity = prodQty,
                    Price = product.Price
                });
            }
            model.TotalAmount = model.OrderItems.Sum(i => i.Price * i.Quantity);
            HttpContext.Session.Set("OrderViewModel", model);
            return RedirectToAction("Create");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Cart()
        {
            var model = HttpContext.Session.Get<OrderViewModel>("OrderViewModel");
            if (model == null || model.OrderItems.Count == 0)
                return RedirectToAction("Create");

            return View(model);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> PlaceOrder()
        {
            var model = HttpContext.Session.Get<OrderViewModel>("OrderViewModel");
            if (model == null || model.OrderItems.Count == 0)
                return RedirectToAction("Create");

            Order order = new Order
            {
                UserId = _userManager.GetUserId(User),
                TotalAmount = model.TotalAmount,
                OrderDate = DateTime.Now,
            };
            foreach (var item in model.OrderItems)
            {
                order.OrderItems.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Price
                });
            }
            await _orders.AddAsync(order);
            HttpContext.Session.Remove("OrderViewModel");
            return RedirectToAction("ViewOrders");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ViewOrders()
        {
            var userId = _userManager.GetUserId(User);
            var userOrders = await _orders.GetAllByIdAsync(userId, "UserId", new QueryOptions<Order>
            {
                Includes = "OrderItems.Product",
            });
            return View(userOrders);
        }

        [HttpPost]
        public IActionResult Increase(int productId)
        {
            var model = HttpContext.Session.Get<OrderViewModel>("OrderViewModel");
            if (model == null)
                return NotFound();
            var item = model.OrderItems.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                item.Quantity++;
                model.TotalAmount = model.OrderItems.Sum(i => i.Price * i.Quantity);
                HttpContext.Session.Set("OrderViewModel", model);
            }
            return RedirectToAction("Cart");
        }

        [HttpPost]
        public IActionResult Decrease(int productId)
        {
            var model = HttpContext.Session.Get<OrderViewModel>("OrderViewModel");
            if (model == null)
                return NotFound();
            var item = model.OrderItems.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                if (item.Quantity > 1)
                {
                    item.Quantity--;
                    model.TotalAmount = model.OrderItems.Sum(i => i.Price * i.Quantity);
                    HttpContext.Session.Set("OrderViewModel", model);
                }
                else
                {
                    model.OrderItems.Remove(item);
                    model.TotalAmount = model.OrderItems.Sum(i => i.Price * i.Quantity);
                    HttpContext.Session.Set("OrderViewModel", model);
                }
            }
            return RedirectToAction("Cart");
        }

        [HttpPost]
        public IActionResult Remove(int productId)
        {
            var model = HttpContext.Session.Get<OrderViewModel>("OrderViewModel");
            if (model == null)
                return NotFound();
            var item = model.OrderItems.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                model.OrderItems.Remove(item);
                model.TotalAmount = model.OrderItems.Sum(i => i.Price * i.Quantity);
                HttpContext.Session.Set("OrderViewModel", model);
            }
            return RedirectToAction("Cart");
        }
        public OrderViewModel GetSessionModel()
        {
            return HttpContext.Session.Get<OrderViewModel>("OrderViewModel") ?? new OrderViewModel();
        }

        private void SaveSessionModel(OrderViewModel model)
        {
            model.TotalAmount = model.OrderItems.Sum(i => i.Price * i.Quantity);
            HttpContext.Session.Set("OrderViewModel", model);
        }
    }
        
    }
