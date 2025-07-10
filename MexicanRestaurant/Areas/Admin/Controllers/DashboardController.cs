using MexicanRestaurant.Core.Interfaces;
using MexicanRestaurant.Core.Models;
using MexicanRestaurant.WebUI.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MexicanRestaurant.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin, Manager")]
    public class DashboardController : Controller
    {
        private readonly IOrderStatisticsService _statistics;
        private readonly IProductService _productService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(IOrderStatisticsService statistics, IProductService productService, UserManager<ApplicationUser> userManager, ILogger<DashboardController> logger)
        {
            _statistics = statistics;
            _productService = productService;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var model = new AdminDashboardViewModel
                {
                    TotalUsers = await _userManager.Users.CountAsync(),
                    TotalProducts = await _productService.GetTotalProductsAsync(),
                    TotalOrders = await _statistics.GetTotalOrdersAsync(startDate, endDate),
                    TotalRevenue = await _statistics.GetTotalRevenueAsync(startDate, endDate),
                    TopProductNames = await _statistics.GetTopProductNamesAsync(5, startDate, endDate),
                    TopProductSales = await _statistics.GetTopProductSalesAsync(5, startDate, endDate),
                    RevenueByDate = await _statistics.GetRevenueByDateAsync(startDate, endDate),
                    StartDate = startDate,
                    EndDate = endDate
                };
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading admin dashboard");
                TempData["ErrorMessage"] = "An error occurred while loading the dashboard. Please try again later.";
                return View(new AdminDashboardViewModel());
            }
        }
    }
}
