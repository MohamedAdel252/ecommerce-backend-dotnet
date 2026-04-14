using ECommerceAPI.DTOs.RecommendationDTOs;
using ECommerceAPI.Models;
using ECommerceAPI.Repositories.Interfaces;
using ECommerceAPI.Services.Interfaces;

namespace ECommerceAPI.Services.Implementations
{
    public class RecommendationService : IRecommendationService
    {
        private readonly IRecommendationRepository _recommendationRepository;

        public RecommendationService(IRecommendationRepository recommendationRepository)
        {
            _recommendationRepository = recommendationRepository;
        }

        public async Task<List<RecommendationResponseDto>> GetForYouRecommendationsAsync(int userId)
        {
            var purchasedProductIds = await _recommendationRepository.GetPurchasedProductIdsAsync(userId);

            if (!purchasedProductIds.Any())
                return await _recommendationRepository.GetLatestInStockProductsAsync(10);

            var purchasedCategoryIds = await _recommendationRepository.GetPurchasedCategoryIdsAsync(purchasedProductIds);
            var recommendationMap = GetRecommendationMap();

            var relatedCategoryIds = purchasedCategoryIds
                .Where(categoryId => recommendationMap.ContainsKey(categoryId))
                .SelectMany(categoryId => recommendationMap[categoryId])
                .Distinct()
                .ToList();

            var allRecommendedCategoryIds = purchasedCategoryIds
                .Union(relatedCategoryIds)
                .Distinct()
                .ToList();

            return await _recommendationRepository.GetRecommendedProductsByCategoriesAsync(
                allRecommendedCategoryIds,
                purchasedProductIds,
                10);
        }

        public async Task<List<RecommendationResponseDto>> GetRelatedProductsAsync(int productId)
        {
            var product = await _recommendationRepository.GetProductWithCategoryAsync(productId);

            if (product == null)
                throw new Exception("Product not found.");

            var recommendationMap = GetRecommendationMap();

            if (!recommendationMap.ContainsKey(product.CategoryId))
                return new List<RecommendationResponseDto>();

            var relatedCategoryIds = recommendationMap[product.CategoryId];

            return await _recommendationRepository.GetProductsByCategoryIdsAsync(relatedCategoryIds, productId, 10);
        }

        public async Task<List<RecommendationResponseDto>> GetBestSellersAsync(int count = 10)
        {
            return await _recommendationRepository.GetBestSellersAsync(count);
        }

        public async Task<List<RecommendationResponseDto>> GetFrequentlyBoughtTogetherAsync(int productId, int count = 5)
        {
            var productIds = await _recommendationRepository.GetFrequentlyBoughtTogetherProductIdsAsync(productId, count);
            return await _recommendationRepository.GetProductsByIdsPreservingOrderAsync(productIds);
        }

        public async Task<HomeFeedResponseDto> GetHomeFeedAsync(int userId)
        {
            var forYou = await GetForYouRecommendationsAsync(userId);
            var bestSellers = await GetBestSellersAsync(10);

            var forYouIds = forYou.Select(p => p.ProductId).ToHashSet();

            bestSellers = bestSellers
                .Where(p => !forYouIds.Contains(p.ProductId))
                .ToList();

            if (forYou.Count < 10)
            {
                var extra = bestSellers.Take(10 - forYou.Count).ToList();
                forYou.AddRange(extra);
            }

            return new HomeFeedResponseDto
            {
                ForYou = forYou,
                BestSellers = bestSellers
            };
        }

        private static Dictionary<int, List<int>> GetRecommendationMap()
        {
            return new Dictionary<int, List<int>>
            {
                { 4, new List<int> { 5 } },
                { 1, new List<int> { 2, 3 } },
                { 2, new List<int> { 3, 1 } },
                { 3, new List<int> { 2, 1 } },
                { 6, new List<int> { 6 } }
            };
        }
    }
}