using ECommerceAPI.Models;
using ECommerceAPI.Repositories.Generic;

namespace ECommerceAPI.Repositories.Interfaces
{
    public interface IOrderRepository : IGenericRepository<Order>
    {
        Task<Order?> GetOrderByIdAsync(int orderId);
        Task<List<Order>> GetOrdersByUserIdAsync(int userId);
        Task<List<Order>> GetAllOrdersAsync();
        Task<Order?> GetByIdWithItemsAndProductsAsync(int id);
        Task<Cart?> GetCartWithItemsAndProductsByUserIdAsync(int userId);
        Task<List<Order>> GetAllOrdersForAdminAsync();
        Task SaveChangesAsync();

        void RemoveCartItemsRange(IEnumerable<CartItem> cartItems);
    }
}