using ECommerceAPI.DTOs.CheckoutDTOs;
using ECommerceAPI.Models;
using ECommerceAPI.Repositories.Implementations;
using ECommerceAPI.Repositories.Interfaces;
using ECommerceAPI.Services.Interfaces;
using ECommerceAPI.Repositories.Generic;

namespace ECommerceAPI.Services.Implementations
{
    public class CheckoutService : ICheckoutService
    {
        private readonly ICartRepository _cartRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IStripeService _stripeService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IProductRepository _productRepository;
        private readonly IEmailService _emailService;

        public CheckoutService(
            ICartRepository cartRepository,
            IOrderRepository orderRepository,
            IProductRepository productRepository,
            IStripeService stripeService,
            IUnitOfWork unitOfWork,
            IEmailService emailService)
        {
            _cartRepository = cartRepository;
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _stripeService = stripeService;
            _unitOfWork = unitOfWork;
            _emailService = emailService;
        }

        public async Task<CheckoutResponseDto> CheckoutAsync(int userId, CreateCheckoutDto dto)
        {
            var cart = await _cartRepository.GetCartByUserIdAsync(userId);

            if (cart == null || cart.CartItems == null || !cart.CartItems.Any())
                throw new Exception("Cart is empty.");

            foreach (var item in cart.CartItems)
            {
                if (item.Product == null)
                    throw new Exception($"Product with ID {item.ProductId} was not found.");

                if (item.Quantity > item.Product.StockQuantity)
                    throw new Exception($"Not enough stock for product: {item.Product.Name}");
            }

            decimal subTotal = cart.CartItems.Sum(item => item.Product!.Price * item.Quantity);
            decimal shippingFee = GetShippingFee(dto.Governorate);
            decimal totalAmount = subTotal + shippingFee;
            //---------
            string? customerEmail = null;

            if (dto.PaymentMethod == PaymentMethod.Cash)
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId);

                if (user == null || string.IsNullOrWhiteSpace(user.Email))
                    throw new Exception("User email was not found.");

                customerEmail = user.Email;
            }

            var productNames = cart.CartItems.ToDictionary(
                x => x.ProductId,
                x => x.Product?.Name ?? "Product"
            );
            //-----------------------------------------------

            var order = new Order
            {
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                Status = OrderStatus.Pending,

                RecipientName = dto.RecipientName,
                PhoneNumber = dto.PhoneNumber,
                Governorate = dto.Governorate,
                AddressLine = dto.AddressLine,
                Notes = dto.Notes,

                PaymentMethod = dto.PaymentMethod,
                PaymentStatus = PaymentStatus.Pending,

                SubTotal = subTotal,
                ShippingFee = shippingFee,
                TotalAmount = totalAmount,
                CustomerEmail = customerEmail,

                OrderItems = cart.CartItems.Select(item => new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.Product!.Price
                }).ToList()
            };

            foreach (var item in cart.CartItems)
            {
                item.Product!.StockQuantity -= item.Quantity;
            }

            await _orderRepository.AddAsync(order);
            _cartRepository.Delete(cart);
            await _unitOfWork.SaveChangesAsync();

            if (dto.PaymentMethod == PaymentMethod.Card)
            {
                var session = await _stripeService.CreateCheckoutSessionAsync(order);

                order.StripeSessionId = session.Id;
                await _unitOfWork.SaveChangesAsync();

                return new CheckoutResponseDto
                {
                    OrderId = order.Id,
                    Message = "Stripe checkout session created successfully.",
                    SubTotal = order.SubTotal,
                    ShippingFee = order.ShippingFee,
                    TotalAmount = order.TotalAmount,
                    PaymentMethod = order.PaymentMethod,
                    PaymentStatus = order.PaymentStatus,
                    OrderStatus = order.Status,
                    CheckoutUrl = session.Url
                };
            }

            if (dto.PaymentMethod == PaymentMethod.Cash && !string.IsNullOrWhiteSpace(order.CustomerEmail))
            {
                var storeName = "Electronics Store";
                var logoUrl = "https://drive.google.com/uc?export=view&id=1v4oo8XLbA8Dig8TyiutlyT8IQflkXxst";

                var productsRows = "";

                foreach (var item in order.OrderItems)
                {
                    var productName = productNames.ContainsKey(item.ProductId)
                        ? productNames[item.ProductId]
                        : "Product";

                    productsRows += $@"
                        <tr>
                            <td style='padding:10px; border:1px solid #eee'>{productName}</td>
                            <td style='padding:10px; border:1px solid #eee'>{item.UnitPrice:0.00} EGP</td>
                            <td style='padding:10px; border:1px solid #eee'>{item.Quantity}</td>
                        </tr>";
                                }

                                var egyptTime = TimeZoneInfo.ConvertTimeFromUtc(
                                    order.CreatedAt,
                                    TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time"));

                                var emailBody = $@"
                        <div style='font-family:Arial, sans-serif; background-color:#f5f7fa; padding:20px'>
                            <div style='max-width:600px; margin:auto; background:white; border-radius:10px; overflow:hidden; box-shadow:0 5px 15px rgba(0,0,0,0.05)'>
                                <div style='background:#0d1b2a; color:white; text-align:center; padding:20px'>
                                    <img src='{logoUrl}' style='max-width:120px; margin-bottom:10px' />
                                    <h2 style='margin:0'>{storeName}</h2>
                                </div>

                                <div style='padding:20px'>
                                    <h2 style='color:#0d1b2a'>Order Confirmed ✅</h2>
                                    <p>Your order <strong>#{order.Id}</strong> has been placed successfully.</p>

                                    <div style='background:#f9fafb; padding:15px; border-radius:8px; margin:15px 0'>
                                        <p><strong>Date:</strong> {egyptTime:dd MMM yyyy - h:mm tt}</p>
                                        <p><strong>Subtotal:</strong> {order.SubTotal:0.00} EGP</p>
                                        <p><strong>Shipping Fee:</strong> {order.ShippingFee:0.00} EGP</p>
                                        <p style='font-weight:bold; font-size:16px; color:#0d1b2a'>
                                            <strong>Total:</strong> {order.TotalAmount:0.00} EGP
                                        </p>
                                        <p><strong>Payment Method:</strong> Cash on Delivery</p>
                                        <p><strong>Payment Status:</strong> Pending</p>
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

                                    <p style='margin-top:20px'>You will pay when the order is delivered 💙</p>
                                </div>

                                <div style='background:#f1f5f9; text-align:center; padding:15px; font-size:12px; color:#666'>
                                    © {DateTime.Now.Year} {storeName} - All rights reserved
                                </div>
                            </div>
                        </div>";

                await _emailService.SendEmailAsync(order.CustomerEmail, "Order Confirmation", emailBody);
            }

            return new CheckoutResponseDto
            {
                OrderId = order.Id,
                Message = "Order placed successfully with cash on delivery.",
                SubTotal = order.SubTotal,
                ShippingFee = order.ShippingFee,
                TotalAmount = order.TotalAmount,
                PaymentMethod = order.PaymentMethod,
                PaymentStatus = order.PaymentStatus,
                OrderStatus = order.Status,
                CheckoutUrl = null
            };
        }

        private decimal GetShippingFee(Governorate governorate)
        {
            if (governorate == Governorate.Cairo || governorate == Governorate.Giza)
                return 100;

            return 200;
        }
    }
}