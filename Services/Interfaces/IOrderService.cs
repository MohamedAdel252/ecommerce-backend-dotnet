using ECommerceAPI.DTOs.OrderDTOs;

namespace ECommerceAPI.Services.Interfaces
{
    public interface IOrderService
    {
        Task CreateOrderAsync(int userId);
        Task<List<OrderResponseDto>> GetMyOrdersAsync(int userId);
        Task<OrderResponseDto?> GetOrderByIdAsync(int userId, int orderId);
        Task<object> CancelOrderAsync(int userId, int orderId);
        Task<List<OrderResponseDto>> GetAllOrdersAsync();
        Task<bool> PatchOrderStatusAsync(int orderId, UpdateOrderStatusDto dto);
    }
}