using ECommerceAPI.DTOs.Product;
using ECommerceAPI.DTOs.ProductDTOs;

namespace ECommerceAPI.Services.Interfaces
{
    public interface IProductService
    {
        Task<List<ProductResponseDto>> GetAllAsync();
        Task<ProductResponseDto?> GetByIdAsync(int id);
        Task<ProductResponseDto> CreateAsync(CreateProductDto dto);
        Task UpdateAsync(int id, UpdateProductDto dto);
        Task DeleteAsync(int id);
        Task<List<ProductResponseDto>> GetByCategoryAsync(int categoryId);
        Task<List<ProductResponseDto>> SearchAsync(string name);
        Task<List<ProductResponseDto>> GetPagedAsync(int page, int pageSize);
        Task<bool> PatchProductAsync(int id, UpdateProductPartialDto dto);
    }
}