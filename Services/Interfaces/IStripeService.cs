using Stripe.Checkout;
using ECommerceAPI.Models;

namespace ECommerceAPI.Services.Interfaces
{
    public interface IStripeService
    {
        public Task<Session> CreateCheckoutSessionAsync(Order order);
    }
}