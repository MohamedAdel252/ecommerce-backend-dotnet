using ECommerceAPI.Models;
using ECommerceAPI.Repositories.Generic;

namespace ECommerceAPI.Repositories.Interfaces
{
    public interface IProductRepository : IGenericRepository<Product>
    {
        Task<bool> CategoryExistsAsync(int categoryId);
        Task<List<Product>> GetByCategoryAsync(int categoryId);
        Task<List<Product>> SearchAsync(string name);
        Task<List<Product>> GetPagedAsync(int page, int pageSize);
    }
}