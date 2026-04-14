using ECommerceAPI.Models;

namespace ECommerceAPI.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<List<Category>> GetAllAsync();
        Task<Category?> GetByIdAsync(int id);
        Task<Category> CreateAsync(Category category);
        Task UpdateAsync(int id, Category category);
        Task DeleteAsync(int id);
    }
}