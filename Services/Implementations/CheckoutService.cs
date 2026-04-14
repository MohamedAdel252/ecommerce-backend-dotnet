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

        public CheckoutService(
            ICartRepository cartRepository,
            IOrderRepository orderRepository,
            IProductRepository productRepository,
            IStripeService stripeService,
            IUnitOfWork unitOfWork  )
        {
            _cartRepository = cartRepository;
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _stripeService = stripeService;
            _unitOfWork = unitOfWork;
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

            await _orderRepository.AddAsync(order); ;
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