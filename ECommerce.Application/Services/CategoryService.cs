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

    // Customer + Admin
    public async Task<IEnumerable<CategoryDto>> GetAllAsync()
    {
        var categories =
            await _categoryRepository.GetAllAsync();

        return categories.Select(MapToDto);
    }

    // Customer + Admin
    public async Task<CategoryDto?> GetByIdAsync(int id)
    {
        var category =
            await _categoryRepository.GetByIdAsync(id);

        if (category == null)
        {
            return null;
        }

        return MapToDto(category);
    }

    // Admin
    public async Task<CategoryDto> CreateAsync(
        CreateCategoryDto categoryDto)
    {
        ValidateCategory(categoryDto);

        var category = new Category
        {
            Name = categoryDto.Name.Trim(),
            Description =
                categoryDto.Description?.Trim()
                ?? string.Empty
        };

        var createdCategory =
            await _categoryRepository.AddAsync(category);

        return MapToDto(createdCategory);
    }

    // Admin
    public async Task<bool> UpdateAsync(
        int id,
        CreateCategoryDto categoryDto)
    {
        ValidateCategory(categoryDto);

        var category =
            await _categoryRepository.GetByIdAsync(id);

        if (category == null)
        {
            return false;
        }

        category.Name = categoryDto.Name.Trim();

        category.Description =
            categoryDto.Description?.Trim()
            ?? string.Empty;

        await _categoryRepository.UpdateAsync(category);

        return true;
    }

    // Admin
    public async Task<bool> DeleteAsync(int id)
    {
        var category =
            await _categoryRepository.GetByIdAsync(id);

        if (category == null)
        {
            return false;
        }

        await _categoryRepository.DeleteAsync(id);

        return true;
    }

    private static void ValidateCategory(
        CreateCategoryDto categoryDto)
    {
        if (string.IsNullOrWhiteSpace(categoryDto.Name))
        {
            throw new ArgumentException(
                "Category name is required.");
        }
    }

    private static CategoryDto MapToDto(
        Category category)
    {
        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description
        };
    }
}