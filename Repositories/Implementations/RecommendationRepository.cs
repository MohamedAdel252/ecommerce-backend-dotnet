using ECommerceAPI.Data;
using ECommerceAPI.DTOs.RecommendationDTOs;
using ECommerceAPI.Models;
using ECommerceAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ECommerceAPI.Repositories.Implementations
{
    public class RecommendationRepository : IRecommendationRepository
    {
        private readonly AppDbContext _context;

        public RecommendationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<int>> GetPurchasedProductIdsAsync(int userId)
        {
            return await _context.Orders
                .Where(o => o.UserId == userId && o.Status == OrderStatus.Completed)
                .SelectMany(o => o.OrderItems)
                .Select(oi => oi.ProductId)
                .Distinct()
                .ToListAsync();
        }

        public async Task<List<int>> GetPurchasedCategoryIdsAsync(List<int> productIds)
        {
            return await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .Select(p => p.CategoryId)
                .Distinct()
                .ToListAsync();
        }

        public async Task<List<RecommendationResponseDto>> GetLatestInStockProductsAsync(int count)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Where(p => p.StockQuantity > 0)
                .OrderByDescending(p => p.Id)
                .Take(count)
                .Select(MapToDto())
                .ToListAsync();
        }

        public async Task<List<RecommendationResponseDto>> GetRecommendedProductsByCategoriesAsync(List<int> categoryIds, List<int> excludedProductIds, int count)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.OrderItems)
                .Where(p => categoryIds.Contains(p.CategoryId)
                            && !excludedProductIds.Contains(p.Id)
                            && p.StockQuantity > 0)
                .OrderByDescending(p => p.OrderItems.Count)
                .ThenByDescending(p => p.Id)
                .Take(count)
                .Select(MapToDto())
                .ToListAsync();
        }

        public async Task<Product?> GetProductWithCategoryAsync(int productId)
        {
            return await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == productId);
        }

        public async Task<List<RecommendationResponseDto>> GetProductsByCategoryIdsAsync(List<int> categoryIds, int excludedProductId, int count)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Where(p => categoryIds.Contains(p.CategoryId)
                            && p.Id != excludedProductId
                            && p.StockQuantity > 0)
                .OrderByDescending(p => p.Id)
                .Take(count)
                .Select(MapToDto())
                .ToListAsync();
        }

        public async Task<List<RecommendationResponseDto>> GetBestSellersAsync(int count)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.OrderItems)
                .Where(p => p.StockQuantity > 0)
                .OrderByDescending(p => p.OrderItems.Sum(oi => oi.Quantity))
                .ThenByDescending(p => p.Id)
                .Take(count)
                .Select(MapToDto())
                .ToListAsync();
        }

        public async Task<List<int>> GetFrequentlyBoughtTogetherProductIdsAsync(int productId, int count)
        {
            return await _context.OrderItems
                .Where(oi => oi.ProductId == productId)
                .SelectMany(oi => oi.Order.OrderItems)
                .Where(oi => oi.ProductId != productId)
                .GroupBy(oi => oi.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .Take(count)
                .Select(x => x.ProductId)
                .ToListAsync();
        }

        public async Task<List<RecommendationResponseDto>> GetProductsByIdsPreservingOrderAsync(List<int> productIds)
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p => productIds.Contains(p.Id) && p.StockQuantity > 0)
                .ToListAsync();

            return products
                .OrderBy(p => productIds.IndexOf(p.Id))
                .Select(p => new RecommendationResponseDto
                {
                    ProductId = p.Id,
                    ProductName = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    ImageUrl = p.ImageUrl,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category != null ? p.Category.Name : string.Empty
                })
                .ToList();
        }

        private static Expression<Func<Product, RecommendationResponseDto>> MapToDto()
        {
            return p => new RecommendationResponseDto
            {
                ProductId = p.Id,
                ProductName = p.Name,
                Description = p.Description,
                Price = p.Price,
                ImageUrl = p.ImageUrl,
                CategoryId = p.CategoryId,
                CategoryName = p.Category != null ? p.Category.Name : string.Empty
            };
        }
    }
}