using MexicanRestaurant.Core.Enums;
using MexicanRestaurant.Core.Interfaces;
using MexicanRestaurant.Core.Models;
using MexicanRestaurant.Core.Specifications;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace MexicanRestaurant.WebUI.Controllers
{
    [Route("webhook")]
    public class StripeWebhookController : Controller
    {
        private readonly IRepository<Order> _orderRepo;
        public StripeWebhookController(IRepository<Order> orderRepo)
        {
            _orderRepo = orderRepo;
        }

        [HttpPost]
        public async Task<IActionResult> Index()
        {
            HttpContext.Request.EnableBuffering();
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var StripeWebhookSecret = Environment.GetEnvironmentVariable("StripeWebhookSecret") ?? throw new InvalidOperationException("Stripe webhook secret not configured.");
            try
            {
                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    StripeWebhookSecret
                );

                if (stripeEvent.Type == "payment_intent.succeeded")
                {
                    var order = await GetOrderFromEvent(stripeEvent);
                    if (order == null)
                        return NotFound("Order not found.");

                    // Update the order status and payment status
                    order.Status = OrderStatus.Confirmed;
                    order.PaymentStatus = PaymentStatus.Succeeded;
                    await _orderRepo.UpdateAsync(order);
                }
                else if (stripeEvent.Type == "payment_intent.payment_failed")
                {
                    var order = await GetOrderFromEvent(stripeEvent);
                    if (order == null)
                        return NotFound("Order not found.");
                    order.Status = OrderStatus.Cancelled;
                    order.PaymentStatus = PaymentStatus.Failed;
                    await _orderRepo.UpdateAsync(order);
                }
                return Ok("Webhook received successfully.");
            }
            catch (StripeException ex)
            {
                return BadRequest($"Stripe error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return BadRequest("Error processing webhook: " + ex.Message);
            }
        }

        private async Task<Order?> GetOrderFromEvent(Event stripeEvent)
        {
            var paymentIntent = (PaymentIntent)stripeEvent.Data.Object;
            if (paymentIntent == null)
                return null;

            var options = new QueryOptions<Order> { Where = o => o.PaymentIntentId == paymentIntent.Id };
            var order = (await _orderRepo.GetAllAsync(options)).FirstOrDefault();
            return order;
        }
    }
}
