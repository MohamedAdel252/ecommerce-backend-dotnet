using ECommerceAPI.DTOs.Product;
using ECommerceAPI.DTOs.ProductDTOs;
using ECommerceAPI.Models;
using ECommerceAPI.Repositories.Implementations;
using ECommerceAPI.Repositories.Interfaces;
using ECommerceAPI.Services.Interfaces;

namespace ECommerceAPI.Services.Implementations
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ProductService(IProductRepository productRepository, IUnitOfWork unitOfWork)
        {
            _productRepository = productRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<List<ProductResponseDto>> GetAllAsync()
        {
            var products = await _productRepository.GetAllAsync();
            return products.Select(MapToDto).ToList();
        }

        public async Task<ProductResponseDto?> GetByIdAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            return product == null ? null : MapToDto(product);
        }

        public async Task<ProductResponseDto> CreateAsync(CreateProductDto dto)
        {
            var categoryExists = await _productRepository.CategoryExistsAsync(dto.CategoryId);

            if (!categoryExists)
                throw new Exception("Invalid category id.");

            var product = new Product
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                StockQuantity = dto.StockQuantity,
                ImageUrl = dto.ImageUrl,
                CategoryId = dto.CategoryId
            };

            await _productRepository.AddAsync(product);
            await _unitOfWork.SaveChangesAsync();

            var createdProduct = await _productRepository.GetByIdAsync(product.Id);
            return createdProduct == null
                ? throw new Exception("Error creating product.")
                : MapToDto(createdProduct);
        }

        public async Task UpdateAsync(int id, UpdateProductDto dto)
        {
            var product = await _productRepository.GetByIdAsync(id);

            if (product == null)
                throw new Exception("Product not found.");

            var categoryExists = await _productRepository.CategoryExistsAsync(dto.CategoryId);
            if (!categoryExists)
                throw new Exception("Invalid CategoryId.");

            product.Name = dto.Name;
            product.Description = dto.Description;
            product.Price = dto.Price;
            product.StockQuantity = dto.StockQuantity;
            product.ImageUrl = dto.ImageUrl;
            product.CategoryId = dto.CategoryId;

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);

            if (product == null)
                throw new Exception("Product not found.");

            await _productRepository.DeleteAsync(product);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<List<ProductResponseDto>> GetByCategoryAsync(int categoryId)
        {
            var products = await _productRepository.GetByCategoryAsync(categoryId);
            return products.Select(MapToDto).ToList();
        }

        public async Task<List<ProductResponseDto>> SearchAsync(string name)
        {
            var products = await _productRepository.SearchAsync(name);
            return products.Select(MapToDto).ToList();
        }

        public async Task<List<ProductResponseDto>> GetPagedAsync(int page, int pageSize)
        {
            var products = await _productRepository.GetPagedAsync(page, pageSize);
            return products.Select(MapToDto).ToList();
        }

        private static ProductResponseDto MapToDto(Product p)
        {
            return new ProductResponseDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                ImageUrl = p.ImageUrl,
                CategoryId = p.CategoryId,
                CategoryName = p.Category?.Name ?? string.Empty
            };
        }

        public async Task<bool> PatchProductAsync(int id, UpdateProductPartialDto dto)
        {
            var product = await _productRepository.GetByIdAsync(id);

            if (product == null)
                return false;

            if (dto.Price.HasValue)
            {
                if (dto.Price < 0)
                    throw new ArgumentException("Price cannot be negative");

                product.Price = dto.Price.Value;
            }

            if (dto.StockQuantity.HasValue)
            {
                if (dto.StockQuantity < 0)
                    throw new ArgumentException("Stock cannot be negative");

                product.StockQuantity = dto.StockQuantity.Value;
            }

            await _productRepository.UpdateAsync(product);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}