using System.Diagnostics;
using MexicanRestaurant.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace MexicanRestaurant.WebUI.Controllers
{
    public class HomeController : Controller
    {

        public HomeController(){}

        public IActionResult Index()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult StatusCode(int code)
        {
            var errorViewModel = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                Message = code switch
                {
                    400 => "A bad request, you have made",
                    401 => "Unauthorized access.",
                    403 => "Forbidden.",
                    404 => "The Page not found.",
                    500 => "An internal server error occurred.",
                    _ => "An unexpected error occurred."
                }
            };
            return View("Error", errorViewModel);
        }
    }
}
