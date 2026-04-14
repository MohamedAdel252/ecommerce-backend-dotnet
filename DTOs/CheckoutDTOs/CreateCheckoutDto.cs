using ECommerceAPI.Models;
using System.ComponentModel.DataAnnotations;
namespace ECommerceAPI.DTOs.CheckoutDTOs
{
    public class CreateCheckoutDto
    {
        [Required]
        [MaxLength(100)]
        public string RecipientName { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        public Governorate Governorate { get; set; }

        [Required]
        [MaxLength(300)]
        public string AddressLine { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Notes { get; set; }

        [Required]
        public PaymentMethod PaymentMethod { get; set; }
    }
}
