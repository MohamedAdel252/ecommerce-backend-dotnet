using Stripe;

namespace ECommerceAPI.Services.Interfaces
{
    public interface IPaymentService
    {
        Task HandleStripeWebhookAsync(Event stripeEvent);
    }
}