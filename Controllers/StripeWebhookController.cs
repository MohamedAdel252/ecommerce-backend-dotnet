using ECommerceAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace ECommerceAPI.Controllers
{
    [Route("api/stripe/webhook")]
    [ApiController]
    public class StripeWebhookController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IPaymentService _paymentService;

        public StripeWebhookController(IConfiguration configuration, IPaymentService paymentService)
        {
            _configuration = configuration;
            _paymentService = paymentService;
        }

        [HttpPost]
        public async Task<IActionResult> HandleWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            try
            {
                var webhookSecret = _configuration["Stripe:WebhookSecret"];

                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    webhookSecret
                );

                await _paymentService.HandleStripeWebhookAsync(stripeEvent);

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest($"Webhook error: {e.Message}");
            }
        }
    }
}