using ECommerceAPI.DTOs.CheckoutDTOs;

namespace ECommerceAPI.Services.Interfaces
{
    public interface ICheckoutService
    {
        Task<CheckoutResponseDto> CheckoutAsync(int userId, CreateCheckoutDto dto);
    }
}