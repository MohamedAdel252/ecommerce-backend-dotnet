using ECommerceAPI.Models;
using ECommerceAPI.Services.Interfaces;
using Microsoft.Extensions.Options;
using Stripe.Checkout;

namespace ECommerceAPI.Services.Implementations
{
    public class StripeService : IStripeService
    {
        private readonly StripeSettings _stripeSettings;

        public StripeService(IOptions<StripeSettings> stripeSettings)
        {
            _stripeSettings = stripeSettings.Value;
        }

        public async Task<Session> CreateCheckoutSessionAsync(Order order)
        {
            var options = new SessionCreateOptions
            {
                Mode = "payment",
                SuccessUrl = "https://electronics-store1.netlify.app/?status=success",
                CancelUrl = "https://electronics-store1.netlify.app/?status=success",
                ClientReferenceId = order.Id.ToString(),
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        Quantity = 1,
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "egp",
                            UnitAmountDecimal = order.TotalAmount * 100,
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = $"Order #{order.Id}"
                            }
                        }
                    }
                },
                Metadata = new Dictionary<string, string>
                {
                    { "orderId", order.Id.ToString() },
                    { "userId", order.UserId.ToString() }
                }
            };

            var service = new SessionService();
            return await service.CreateAsync(options);
        }
    }
}
