using ECommerce.Application.DTOs;

namespace ECommerce.Application.Services;

public interface ICategoryService
{
    Task<IEnumerable<CategoryDto>> GetAllAsync();

    Task<CategoryDto?> GetByIdAsync(int id);

    Task<CategoryDto> CreateAsync(CreateCategoryDto categoryDto);

    Task<bool> UpdateAsync(int id, CreateCategoryDto categoryDto);

    Task<bool> DeleteAsync(int id);
}