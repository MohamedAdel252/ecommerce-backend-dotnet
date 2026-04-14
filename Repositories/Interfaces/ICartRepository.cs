using ECommerceAPI.Models;
using ECommerceAPI.Repositories.Generic;

namespace ECommerceAPI.Repositories.Interfaces
{
    public interface ICartRepository : IGenericRepository<Cart>
    {
        Task<Cart?> GetCartByUserIdAsync(int userId);
        Task<Product?> GetProductByIdAsync(int productId);
        Task<CartItem?> GetCartItemByIdForUserAsync(int userId, int cartItemId);
        Task AddCartItemAsync(CartItem cartItem);
        void RemoveCartItem(CartItem cartItem);
        void RemoveCartItemsRange(IEnumerable<CartItem> cartItems);
        void UpdateProduct(Product product);
    }
}