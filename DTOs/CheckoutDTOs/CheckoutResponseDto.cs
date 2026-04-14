using ECommerceAPI.Models;
namespace ECommerceAPI.DTOs.CheckoutDTOs
{
    public class CheckoutResponseDto
    {
        public int OrderId { get; set; }
        public string Message { get; set; } = string.Empty;

        public decimal SubTotal { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal TotalAmount { get; set; }

        public PaymentMethod PaymentMethod { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public OrderStatus OrderStatus { get; set; }

        public string? CheckoutUrl { get; set; }
    }
}
