using ECommerceAPI.DTOs.CartDTOs;
using ECommerceAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECommerceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        [HttpGet]
        public async Task<ActionResult<CartResponseDto>> GetMyCart()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
                return Unauthorized();

            int userId = int.Parse(userIdClaim.Value);

            var result = await _cartService.GetMyCartAsync(userId);
            return Ok(result);
        }

        [HttpPost("add")]
        public async Task<ActionResult> AddToCart([FromBody] AddToCartDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
                return Unauthorized();

            int userId = int.Parse(userIdClaim.Value);

            try
            {
                await _cartService.AddToCartAsync(userId, dto);
                return Ok("Product added to cart successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("remove/{cartItemId}")]
        public async Task<ActionResult> RemoveFromCart(int cartItemId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
                return Unauthorized();

            int userId = int.Parse(userIdClaim.Value);

            try
            {
                await _cartService.RemoveFromCartAsync(userId, cartItemId);
                return Ok("Product removed from cart successfully.");
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPatch("items/{cartItemId}")]
        public async Task<ActionResult> UpdateCartItemQuantity(int cartItemId, [FromBody] UpdateCartItemQuantityDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
                return Unauthorized();

            int userId = int.Parse(userIdClaim.Value);

            try
            {
                await _cartService.UpdateCartItemAsync(userId, cartItemId, dto);
                return Ok("Cart item quantity updated successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("clear")]
        public async Task<ActionResult> ClearCart()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
                return Unauthorized();

            int userId = int.Parse(userIdClaim.Value);

            try
            {
                await _cartService.ClearCartAsync(userId);
                return Ok("Cart cleared successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}