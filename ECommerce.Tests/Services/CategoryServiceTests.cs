using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Services;
using ECommerce.Domain.Entities;
using Moq;

namespace ECommerce.Tests.Services;

public class CategoryServiceTests
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly CategoryService _categoryService;

    public CategoryServiceTests()
    {
        _categoryRepositoryMock =
            new Mock<ICategoryRepository>();

        _categoryService =
            new CategoryService(
                _categoryRepositoryMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_CategoryExists_ReturnsCategory()
    {
        // Arrange
        var category = new Category
        {
            Id = 1,
            Name = "Electronics",
            Description = "Electronic devices"
        };

        _categoryRepositoryMock
            .Setup(repository =>
                repository.GetByIdAsync(1))
            .ReturnsAsync(category);

        // Act
        var result =
            await _categoryService.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Electronics", result.Name);
        Assert.Equal(
            "Electronic devices",
            result.Description);
    }

    [Fact]
    public async Task GetByIdAsync_CategoryDoesNotExist_ReturnsNull()
    {
        // Arrange
        _categoryRepositoryMock
            .Setup(repository =>
                repository.GetByIdAsync(999))
            .ReturnsAsync((Category?)null);

        // Act
        var result =
            await _categoryService.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_ValidCategory_ReturnsCreatedCategory()
    {
        // Arrange
        var dto = new CreateCategoryDto
        {
            Name = "Accessories",
            Description = "Computer accessories"
        };

        _categoryRepositoryMock
            .Setup(repository =>
                repository.AddAsync(
                    It.IsAny<Category>()))
            .ReturnsAsync(
                (Category category) =>
                {
                    category.Id = 10;
                    return category;
                });

        // Act
        var result =
            await _categoryService.CreateAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(10, result.Id);
        Assert.Equal(
            "Accessories",
            result.Name);
        Assert.Equal(
            "Computer accessories",
            result.Description);

        _categoryRepositoryMock.Verify(
            repository =>
                repository.AddAsync(
                    It.IsAny<Category>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_EmptyName_ThrowsArgumentException()
    {
        // Arrange
        var dto = new CreateCategoryDto
        {
            Name = "",
            Description = "Invalid category"
        };

        // Act & Assert
        var exception =
            await Assert.ThrowsAsync<ArgumentException>(
                () => _categoryService.CreateAsync(dto));

        Assert.Equal(
            "Category name is required.",
            exception.Message);
    }

    [Fact]
    public async Task UpdateAsync_CategoryExists_ReturnsTrue()
    {
        // Arrange
        var category = new Category
        {
            Id = 1,
            Name = "Electronics",
            Description = "Old description"
        };

        var dto = new CreateCategoryDto
        {
            Name = "Updated Electronics",
            Description = "Updated description"
        };

        _categoryRepositoryMock
            .Setup(repository =>
                repository.GetByIdAsync(1))
            .ReturnsAsync(category);

        // Act
        var result =
            await _categoryService.UpdateAsync(1, dto);

        // Assert
        Assert.True(result);
        Assert.Equal(
            "Updated Electronics",
            category.Name);
        Assert.Equal(
            "Updated description",
            category.Description);

        _categoryRepositoryMock.Verify(
            repository =>
                repository.UpdateAsync(category),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_CategoryDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var dto = new CreateCategoryDto
        {
            Name = "Updated Category",
            Description = "Description"
        };

        _categoryRepositoryMock
            .Setup(repository =>
                repository.GetByIdAsync(999))
            .ReturnsAsync((Category?)null);

        // Act
        var result =
            await _categoryService.UpdateAsync(999, dto);

        // Assert
        Assert.False(result);

        _categoryRepositoryMock.Verify(
            repository =>
                repository.UpdateAsync(
                    It.IsAny<Category>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_CategoryExists_ReturnsTrue()
    {
        // Arrange
        var category = new Category
        {
            Id = 1,
            Name = "Electronics",
            Description = "Electronic devices"
        };

        _categoryRepositoryMock
            .Setup(repository =>
                repository.GetByIdAsync(1))
            .ReturnsAsync(category);

        // Act
        var result =
            await _categoryService.DeleteAsync(1);

        // Assert
        Assert.True(result);

        _categoryRepositoryMock.Verify(
            repository =>
                repository.DeleteAsync(1),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_CategoryDoesNotExist_ReturnsFalse()
    {
        // Arrange
        _categoryRepositoryMock
            .Setup(repository =>
                repository.GetByIdAsync(999))
            .ReturnsAsync((Category?)null);

        // Act
        var result =
            await _categoryService.DeleteAsync(999);

        // Assert
        Assert.False(result);

        _categoryRepositoryMock.Verify(
            repository =>
                repository.DeleteAsync(999),
            Times.Never);
    }
}