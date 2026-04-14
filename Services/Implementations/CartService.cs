using ECommerceAPI.DTOs.CartDTOs;
using ECommerceAPI.Models;
using ECommerceAPI.Repositories.Implementations;
using ECommerceAPI.Repositories.Interfaces;
using ECommerceAPI.Services.Interfaces;

namespace ECommerceAPI.Services.Implementations
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CartService(ICartRepository cartRepository, IUnitOfWork unitOfWork   )
        {
            _cartRepository = cartRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<CartResponseDto> GetMyCartAsync(int userId)
        {
            var cart = await _cartRepository.GetCartByUserIdAsync(userId);

            if (cart == null)
            {
                return new CartResponseDto
                {
                    CartId = 0,
                    UserId = userId,
                    Items = new List<CartItemResponseDto>(),
                    TotalAmount = 0
                };
            }

            return new CartResponseDto
            {
                CartId = cart.Id,
                UserId = userId,
                Items = cart.CartItems.Select(ci => new CartItemResponseDto
                {
                    CartItemId = ci.Id,
                    ProductId = ci.ProductId,
                    ProductName = ci.Product?.Name ?? string.Empty,
                    Price = ci.Product?.Price ?? 0,
                    Quantity = ci.Quantity,
                    TotalPrice = (ci.Product?.Price ?? 0) * ci.Quantity
                }).ToList(),
                TotalAmount = cart.CartItems.Sum(ci => (ci.Product?.Price ?? 0) * ci.Quantity)
            };
        }

        public async Task AddToCartAsync(int userId, AddToCartDto dto)
        {
            if (dto.Quantity <= 0)
                throw new Exception("Quantity must be greater than 0.");

            var product = await _cartRepository.GetProductByIdAsync(dto.ProductId);

            if (product == null)
                throw new Exception("Product not found.");

            if (dto.Quantity > product.StockQuantity)
                throw new Exception("Requested quantity exceeds available stock.");

            var cart = await _cartRepository.GetCartByUserIdAsync(userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                await _cartRepository.AddCartAsync(cart);
                await _unitOfWork.SaveChangesAsync();

                cart = await _cartRepository.GetCartByUserIdAsync(userId)
                    ?? throw new Exception("Failed to create cart.");
            }

            var existingItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == dto.ProductId);

            if (existingItem != null)
            {
                if (existingItem.Quantity + dto.Quantity > product.StockQuantity)
                    throw new Exception("Requested quantity exceeds available stock.");

                existingItem.Quantity += dto.Quantity;
            }
            else
            {
                await _cartRepository.AddCartItemAsync(new CartItem
                {
                    CartId = cart.Id,
                    ProductId = dto.ProductId,
                    Quantity = dto.Quantity
                });
            }

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task RemoveFromCartAsync(int userId, int cartItemId)
        {
            var cartItem = await _cartRepository.GetCartItemByIdForUserAsync(userId, cartItemId);

            if (cartItem == null)
                throw new Exception("Cart item not found.");

            _cartRepository.RemoveCartItem(cartItem);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateCartItemAsync(int userId, int cartItemId, UpdateCartItemQuantityDto dto)
        {
            if (dto.Quantity <= 0)
                throw new Exception("Quantity must be greater than 0.");

            var cartItem = await _cartRepository.GetCartItemByIdForUserAsync(userId, cartItemId);

            if (cartItem == null)
                throw new Exception("Cart item not found.");

            if (cartItem.Product == null)
                throw new Exception("Product not found.");

            if (dto.Quantity > cartItem.Product.StockQuantity)
                throw new Exception("Requested quantity exceeds available stock.");

            cartItem.Quantity = dto.Quantity;
            await _unitOfWork.SaveChangesAsync(); ;
        }

        public async Task ClearCartAsync(int userId)
        {
            var cart = await _cartRepository.GetCartByUserIdAsync(userId);

            if (cart == null || !cart.CartItems.Any())
                throw new Exception("Cart is already empty.");

            _cartRepository.RemoveCartItemsRange(cart.CartItems);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}