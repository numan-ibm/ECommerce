using ECommerce.Application.DTOs;

namespace ECommerce.Application.Interfaces;

public interface ICategoryService
{
    // Customer operations
    Task<IEnumerable<CategoryDto>> GetAllAsync();
    Task<CategoryDto?> GetByIdAsync(int id);

    // Admin operations
    Task<CategoryDto> CreateAsync(CreateCategoryDto categoryDto);
    Task<bool> UpdateAsync(int id, CreateCategoryDto categoryDto);
    Task<bool> DeleteAsync(int id);
}