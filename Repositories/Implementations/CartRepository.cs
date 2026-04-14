using ECommerceAPI.Data;
using ECommerceAPI.Models;
using ECommerceAPI.Repositories;
using ECommerceAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using ECommerceAPI.Repositories.Generic;

namespace ECommerceAPI.Repositories.Implementations
{
    public class CartRepository : GenericRepository<Cart>, ICartRepository
    {
        private readonly AppDbContext _context;

        public CartRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Cart?> GetCartByUserIdAsync(int userId)
        {
            return await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }

        public async Task<Product?> GetProductByIdAsync(int productId)
        {
            return await _context.Products
                .FirstOrDefaultAsync(p => p.Id == productId);
        }

        public async Task<CartItem?> GetCartItemByIdForUserAsync(int userId, int cartItemId)
        {
            return await _context.CartItems
                .Include(ci => ci.Cart)
                .Include(ci => ci.Product)
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId &&
                                           ci.Cart != null &&
                                           ci.Cart.UserId == userId);
        }

        public async Task AddCartItemAsync(CartItem cartItem)
        {
            await _context.CartItems.AddAsync(cartItem);
        }

        public void RemoveCartItem(CartItem cartItem)
        {
            _context.CartItems.Remove(cartItem);
        }

        public void RemoveCartItemsRange(IEnumerable<CartItem> cartItems)
        {
            _context.CartItems.RemoveRange(cartItems);
        }

        public void UpdateProduct(Product product)
        {
            _context.Products.Update(product);
        }
    }
}