using ECommerceAPI.Models;
using ECommerceAPI.Repositories.Implementations;
using ECommerceAPI.Repositories.Interfaces;
using ECommerceAPI.Services.Interfaces;

namespace ECommerceAPI.Services.Implementations
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CategoryService(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork)
        {
            _categoryRepository = categoryRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<List<Category>> GetAllAsync()
        {
            return await _categoryRepository.GetAllAsync();
        }

        public async Task<Category?> GetByIdAsync(int id)
        {
            return await _categoryRepository.GetByIdAsync(id);
        }

        public async Task<Category> CreateAsync(Category category)
        {
            await _categoryRepository.AddAsync(category);
            await _unitOfWork.SaveChangesAsync();
            return category;
        }

        public async Task UpdateAsync(int id, Category updatedCategory)
        {
            var category = await _categoryRepository.GetByIdAsync(id);

            if (category == null)
                throw new Exception("Category not found.");

            category.Name = updatedCategory.Name;

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);

            if (category == null)
                throw new Exception("Category not found.");

            await _categoryRepository.DeleteAsync(category);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}