using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<IEnumerable<CategoryDto>> GetAllAsync()
    {
        var categories = await _categoryRepository.GetAllAsync();

        return categories.Select(MapToDto);
    }

    public async Task<CategoryDto?> GetByIdAsync(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);

        return category is null ? null : MapToDto(category);
    }

    public async Task<CategoryDto> CreateAsync(
        CreateCategoryDto categoryDto)
    {
        var category = new Category
        {
            Name = categoryDto.Name,
            Description = categoryDto.Description
        };

        var createdCategory =
            await _categoryRepository.AddAsync(category);

        return MapToDto(createdCategory);
    }

    public async Task<bool> UpdateAsync(
        int id,
        CreateCategoryDto categoryDto)
    {
        var category = await _categoryRepository.GetByIdAsync(id);

        if (category is null)
        {
            return false;
        }

        category.Name = categoryDto.Name;
        category.Description = categoryDto.Description;

        await _categoryRepository.UpdateAsync(category);

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);

        if (category is null)
        {
            return false;
        }

        await _categoryRepository.DeleteAsync(id);

        return true;
    }

    private static CategoryDto MapToDto(Category category)
    {
        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description
        };
    }
}