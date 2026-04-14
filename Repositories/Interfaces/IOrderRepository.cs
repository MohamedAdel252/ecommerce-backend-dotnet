using ECommerceAPI.Models;

namespace ECommerceAPI.Repositories.Interfaces
{
    public interface IOrderRepository
    {
        Task AddOrderAsync(Order order);
        Task<Order?> GetOrderByIdAsync(int orderId);
        Task<List<Order>> GetOrdersByUserIdAsync(int userId);
        Task<List<Order>> GetAllOrdersAsync();
        Task<Order?> GetByIdAsync(int id);
        Task<Order?> GetByIdWithItemsAndProductsAsync(int id);
        Task<Cart?> GetCartWithItemsAndProductsByUserIdAsync(int userId);
        void RemoveCartItemsRange(IEnumerable<CartItem> cartItems);
        Task UpdateAsync(Order order);
    }
}