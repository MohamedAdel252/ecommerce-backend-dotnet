using ECommerceAPI.DTOs.RecommendationDTOs;
using ECommerceAPI.Models;

namespace ECommerceAPI.Repositories.Interfaces
{
    public interface IRecommendationRepository
    {
        Task<List<int>> GetPurchasedProductIdsAsync(int userId);
        Task<List<int>> GetPurchasedCategoryIdsAsync(List<int> productIds);
        Task<List<RecommendationResponseDto>> GetLatestInStockProductsAsync(int count);
        Task<List<RecommendationResponseDto>> GetRecommendedProductsByCategoriesAsync(List<int> categoryIds, List<int> excludedProductIds, int count);
        Task<Product?> GetProductWithCategoryAsync(int productId);
        Task<List<RecommendationResponseDto>> GetProductsByCategoryIdsAsync(List<int> categoryIds, int excludedProductId, int count);
        Task<List<RecommendationResponseDto>> GetBestSellersAsync(int count);
        Task<List<int>> GetFrequentlyBoughtTogetherProductIdsAsync(int productId, int count);
        Task<List<RecommendationResponseDto>> GetProductsByIdsPreservingOrderAsync(List<int> productIds);
    }
}