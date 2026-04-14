namespace ECommerceAPI.DTOs.RecommendationDTOs
{
    public class HomeFeedResponseDto
    {
        public List<RecommendationResponseDto> ForYou { get; set; } = new();
        public List<RecommendationResponseDto> BestSellers { get; set; } = new();
    }
}