using ECommerceAPI.DTOs.RecommendationDTOs;
using ECommerceAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECommerceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RecommendationsController : ControllerBase
    {
        private readonly IRecommendationService _recommendationService;

        public RecommendationsController(IRecommendationService recommendationService)
        {
            _recommendationService = recommendationService;
        }

        [HttpGet("for-you")]
        public async Task<ActionResult<IEnumerable<RecommendationResponseDto>>> GetForYouRecommendations()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
                return Unauthorized();

            int userId = int.Parse(userIdClaim.Value);

            var result = await _recommendationService.GetForYouRecommendationsAsync(userId);
            return Ok(result);
        }

        [HttpGet("related/{productId}")]
        public async Task<ActionResult<IEnumerable<RecommendationResponseDto>>> GetRelatedProducts(int productId)
        {
            try
            {
                var result = await _recommendationService.GetRelatedProductsAsync(productId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("best-sellers")]
        public async Task<ActionResult<IEnumerable<RecommendationResponseDto>>> GetBestSellers([FromQuery] int count = 10)
        {
            var result = await _recommendationService.GetBestSellersAsync(count);
            return Ok(result);
        }

        [HttpGet("frequently-bought/{productId}")]
        public async Task<ActionResult<IEnumerable<RecommendationResponseDto>>> GetFrequentlyBought(int productId, [FromQuery] int count = 5)
        {
            var result = await _recommendationService.GetFrequentlyBoughtTogetherAsync(productId, count);
            return Ok(result);
        }

        [HttpGet("home-feed")]
        public async Task<ActionResult<HomeFeedResponseDto>> GetHomeFeed()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
                return Unauthorized();

            int userId = int.Parse(userIdClaim.Value);

            var result = await _recommendationService.GetHomeFeedAsync(userId);
            return Ok(result);
        }
    }
}