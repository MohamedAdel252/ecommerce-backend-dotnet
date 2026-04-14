using ECommerceAPI.DTOs.Product;
using ECommerceAPI.DTOs.ProductDTOs;
using ECommerceAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ECommerceAPI.Services.Implementations;

namespace ECommerceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _service;

        public ProductsController(IProductService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductResponseDto>>> getAll()
        {
            return Ok(await _service.GetAllAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductResponseDto>> getById(int id)
        {
            var product = await _service.GetByIdAsync(id);
            if (product == null) return NotFound();
            return Ok(product);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<ProductResponseDto>> Create(CreateProductDto dto)
        {
            try
            {
                var product = await _service.CreateAsync(dto);
                return CreatedAtAction(nameof(getById), new { id = product.Id }, product);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateProductDto dto)
        {
            try
            {
                await _service.UpdateAsync(id, dto);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _service.DeleteAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("category/{categoryId}")]
        public async Task<ActionResult<IEnumerable<ProductResponseDto>>> getByCategory(int categoryId)
        {
            return Ok(await _service.GetByCategoryAsync(categoryId));
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<ProductResponseDto>>> search(string name)
        {
            return Ok(await _service.SearchAsync(name));
        }

        [HttpGet("paged")]
        public async Task<ActionResult<IEnumerable<ProductResponseDto>>> GetPaged(int page = 1, int pageSize = 10)
        {
            return Ok(await _service.GetPagedAsync(page, pageSize));
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchProduct(int id, [FromBody] UpdateProductPartialDto dto)
        {
            if (dto == null)
                return BadRequest("Invalid data");

            if (!dto.Price.HasValue && !dto.StockQuantity.HasValue)
                return BadRequest("Provide at least one field");

            try
            {
                var result = await _service.PatchProductAsync(id, dto);

                if (!result)
                    return NotFound("Product not found");

                return Ok(new { message = "Product updated successfully" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}