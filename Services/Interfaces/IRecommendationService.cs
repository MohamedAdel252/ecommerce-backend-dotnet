using ECommerceAPI.DTOs.RecommendationDTOs;

namespace ECommerceAPI.Services.Interfaces
{
    public interface IRecommendationService
    {
        Task<List<RecommendationResponseDto>> GetForYouRecommendationsAsync(int userId);
        Task<List<RecommendationResponseDto>> GetRelatedProductsAsync(int productId);
        Task<List<RecommendationResponseDto>> GetBestSellersAsync(int count = 10);
        Task<List<RecommendationResponseDto>> GetFrequentlyBoughtTogetherAsync(int productId, int count = 5);
        Task<HomeFeedResponseDto> GetHomeFeedAsync(int userId);
    }
}