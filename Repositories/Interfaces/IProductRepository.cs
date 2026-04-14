using ECommerceAPI.Models;

namespace ECommerceAPI.Repositories.Interfaces
{
    public interface IProductRepository
    {
        Task<List<Product>> GetAllAsync();
        Task<Product?> GetByIdAsync(int id);
        Task<bool> CategoryExistsAsync(int categoryId);
        Task AddAsync(Product product);
        Task DeleteAsync(Product product);
        Task<List<Product>> GetByCategoryAsync(int categoryId);
        Task<List<Product>> SearchAsync(string name);
        Task<List<Product>> GetPagedAsync(int page, int pageSize);
        Task UpdateAsync(Product product);
    }
}