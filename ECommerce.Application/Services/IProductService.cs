using ECommerce.Application.DTOs;

namespace ECommerce.Application.Services;

public interface IProductService
{
    Task<IEnumerable<ProductDto>> GetAllAsync();

    Task<ProductDto?> GetByIdAsync(int id);

    Task<ProductDto> CreateAsync(CreateProductDto productDto);

    Task<bool> UpdateAsync(int id, CreateProductDto productDto);

    Task<bool> DeleteAsync(int id);
}