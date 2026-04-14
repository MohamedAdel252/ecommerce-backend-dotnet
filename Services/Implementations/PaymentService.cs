using ECommerceAPI.Models;
using ECommerceAPI.Repositories.Implementations;
using ECommerceAPI.Repositories.Interfaces;
using ECommerceAPI.Services.Interfaces;
using Stripe;
using Stripe.Checkout;

namespace ECommerceAPI.Services.Implementations
{
    public class PaymentService : IPaymentService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IEmailService _emailService;
        private readonly IUnitOfWork _unitOfWork;

        public PaymentService(IOrderRepository orderRepository, IEmailService emailService, IUnitOfWork unitOfWork)
        {
            _orderRepository = orderRepository;
            _emailService = emailService;
            _unitOfWork = unitOfWork;
        }

        public async Task HandleStripeWebhookAsync(Event stripeEvent)
        {
            if (stripeEvent.Type != "checkout.session.completed")
                return;

            var session = stripeEvent.Data.Object as Session;

            if (session == null)
                return;

            var customerEmail = session.CustomerDetails?.Email;

            int orderId = 0;

            if (!string.IsNullOrEmpty(session.ClientReferenceId))
            {
                orderId = int.Parse(session.ClientReferenceId);
            }
            else if (session.Metadata != null && session.Metadata.ContainsKey("orderId"))
            {
                orderId = int.Parse(session.Metadata["orderId"]);
            }

            if (orderId <= 0)
                return;

            var order = await _orderRepository.GetByIdWithItemsAndProductsAsync(orderId);

            if (order == null)
                return;

            order.PaymentStatus = PaymentStatus.Paid;
            order.StripeSessionId = session.Id;
            order.CustomerEmail = customerEmail;

            await _unitOfWork.SaveChangesAsync();

            if (string.IsNullOrEmpty(customerEmail))
                return;

            var storeName = "Electronics Store";
            var logoUrl = "https://drive.google.com/uc?export=view&id=1v4oo8XLbA8Dig8TyiutlyT8IQflkXxst";

            var productsRows = "";

            foreach (var item in order.OrderItems)
            {
                productsRows += $@"
                <tr>
                    <td style='padding:10px; border:1px solid #eee'>{item.Product?.Name}</td>
                    <td style='padding:10px; border:1px solid #eee'>{item.UnitPrice:0.00} EGP</td>
                    <td style='padding:10px; border:1px solid #eee'>{item.Quantity}</td>
                </tr>";
            }

            var egyptTime = TimeZoneInfo.ConvertTimeFromUtc(
                order.CreatedAt,
                TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time"));

            decimal shippingFee = order.Governorate == Governorate.Cairo || order.Governorate == Governorate.Giza ? 100 : 200;
            decimal subtotal = order.TotalAmount - shippingFee;

            var emailBody = $@"
                <div style='font-family:Arial, sans-serif; background-color:#f5f7fa; padding:20px'>
                    <div style='max-width:600px; margin:auto; background:white; border-radius:10px; overflow:hidden; box-shadow:0 5px 15px rgba(0,0,0,0.05)'>
                        <div style='background:#0d1b2a; color:white; text-align:center; padding:20px'>
                            <img src='{logoUrl}' style='max-width:120px; margin-bottom:10px' />
                            <h2 style='margin:0'>{storeName}</h2>
                        </div>

                        <div style='padding:20px'>
                            <h2 style='color:#0d1b2a'>Payment Confirmed 🎉</h2>
                            <p>Your order <strong>#{order.Id}</strong> has been successfully paid.</p>

                            <div style='background:#f9fafb; padding:15px; border-radius:8px; margin:15px 0'>
                                <p><strong>Date:</strong> {egyptTime:dd MMM yyyy - h:mm tt}</p>
                                <p><strong>Subtotal:</strong> {subtotal:0.00} EGP</p>
                                <p><strong>Shipping Fee:</strong> {shippingFee:0.00} EGP</p>
                                <p style='font-weight:bold; font-size:16px; color:#0d1b2a'>
                                    <strong>Total Paid:</strong> {order.TotalAmount:0.00} EGP
                                </p>
                                <p><strong>Payment Method:</strong> Card</p>
                            </div>

                            <h3>Order Items</h3>

                            <table style='width:100%; border-collapse:collapse'>
                                <thead>
                                    <tr style='background:#f1f5f9'>
                                        <th style='padding:10px; border:1px solid #eee'>Product</th>
                                        <th style='padding:10px; border:1px solid #eee'>Price</th>
                                        <th style='padding:10px; border:1px solid #eee'>Qty</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {productsRows}
                                </tbody>
                            </table>

                            <h3 style='margin-top:20px'>Delivery Details</h3>

                            <div style='background:#f9fafb; padding:15px; border-radius:8px'>
                                <p><strong>Name:</strong> {order.RecipientName}</p>
                                <p><strong>Phone:</strong> {order.PhoneNumber}</p>
                                <p><strong>Governorate:</strong> {order.Governorate}</p>
                                <p><strong>Address:</strong> {order.AddressLine}</p>
                            </div>

                            <p style='margin-top:20px'>Thank you for shopping with us 💙</p>
                        </div>

                        <div style='background:#f1f5f9; text-align:center; padding:15px; font-size:12px; color:#666'>
                            © {DateTime.Now.Year} {storeName} - All rights reserved
                        </div>
                    </div>
                </div>";

            await _emailService.SendEmailAsync(customerEmail, "Order Confirmation", emailBody);
        }
    }
}