using ECommerceAPI.DTOs.CartDTOs;

namespace ECommerceAPI.Services.Interfaces
{
    public interface ICartService
    {
        Task<CartResponseDto> GetMyCartAsync(int userId);
        Task AddToCartAsync(int userId, AddToCartDto dto);
        Task RemoveFromCartAsync(int userId, int cartItemId);
        Task UpdateCartItemAsync(int userId, int cartItemId, UpdateCartItemQuantityDto dto);
        Task ClearCartAsync(int userId);
    }
}