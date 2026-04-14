using ECommerceAPI.Models;

namespace ECommerceAPI.Repositories.Interfaces
{
    public interface ICartRepository
    {
        Task<Cart?> GetCartByUserIdAsync(int userId);
        Task<Product?> GetProductByIdAsync(int productId);
        Task<CartItem?> GetCartItemByIdForUserAsync(int userId, int cartItemId);
        Task AddCartAsync(Cart cart);
        Task AddCartItemAsync(CartItem cartItem);
        void RemoveCart(Cart cart);
        void RemoveCartItem(CartItem cartItem);
        void RemoveCartItemsRange(IEnumerable<CartItem> cartItems);
    }
}