using ECommerce.Application.DTOs;
using ECommerce.Application.Services;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using Moq;

namespace ECommerce.Tests.Services;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly ProductService _productService;

    public ProductServiceTests()
    {
        _productRepositoryMock =
            new Mock<IProductRepository>();

        _cacheServiceMock =
            new Mock<ICacheService>();

        _productService =
            new ProductService(
                _productRepositoryMock.Object,
                _cacheServiceMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ProductExists_ReturnsProduct()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            Name = "Laptop",
            Description = "Test laptop",
            Price = 999.99m,
            CategoryId = 1,
            StockQuantity = 10
        };

        _cacheServiceMock
            .Setup(cache =>
                cache.GetAsync<ProductDto>("product:1"))
            .ReturnsAsync((ProductDto?)null);

        _productRepositoryMock
            .Setup(repository =>
                repository.GetByIdAsync(1))
            .ReturnsAsync(product);

        // Act
        var result =
            await _productService.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Laptop", result.Name);
        Assert.Equal(999.99m, result.Price);
        Assert.Equal(10, result.StockQuantity);
    }

    [Fact]
    public async Task GetByIdAsync_ProductDoesNotExist_ReturnsNull()
    {
        // Arrange
        _cacheServiceMock
            .Setup(cache =>
                cache.GetAsync<ProductDto>("product:999"))
            .ReturnsAsync((ProductDto?)null);

        _productRepositoryMock
            .Setup(repository =>
                repository.GetByIdAsync(999))
            .ReturnsAsync((Product?)null);

        // Act
        var result =
            await _productService.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_ValidProduct_ReturnsCreatedProduct()
    {
        // Arrange
        var dto = new CreateProductDto
        {
            Name = "Wireless Mouse",
            Description = "Test mouse",
            Price = 39.99m,
            CategoryId = 1,
            StockQuantity = 25
        };

        _productRepositoryMock
            .Setup(repository =>
                repository.AddAsync(
                    It.IsAny<Product>()))
            .ReturnsAsync(
                (Product product) =>
                {
                    product.Id = 10;
                    return product;
                });

        // Act
        var result =
            await _productService.CreateAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(10, result.Id);
        Assert.Equal(
            "Wireless Mouse",
            result.Name);
        Assert.Equal(
            39.99m,
            result.Price);
        Assert.Equal(
            25,
            result.StockQuantity);

        _productRepositoryMock.Verify(
            repository =>
                repository.AddAsync(
                    It.IsAny<Product>()),
            Times.Once);

        _cacheServiceMock.Verify(
            cache =>
                cache.RemoveAsync("products:all"),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_EmptyName_ThrowsArgumentException()
    {
        // Arrange
        var dto = new CreateProductDto
        {
            Name = "",
            Description = "Invalid product",
            Price = 10m,
            CategoryId = 1,
            StockQuantity = 5
        };

        // Act & Assert
        var exception =
            await Assert.ThrowsAsync<ArgumentException>(
                () => _productService.CreateAsync(dto));

        Assert.Equal(
            "Product name is required.",
            exception.Message);
    }

    [Fact]
    public async Task CreateAsync_NegativePrice_ThrowsArgumentException()
    {
        // Arrange
        var dto = new CreateProductDto
        {
            Name = "Invalid Product",
            Description = "Negative price",
            Price = -10m,
            CategoryId = 1,
            StockQuantity = 5
        };

        // Act & Assert
        var exception =
            await Assert.ThrowsAsync<ArgumentException>(
                () => _productService.CreateAsync(dto));

        Assert.Equal(
            "Product price cannot be negative.",
            exception.Message);
    }

    [Fact]
    public async Task CreateAsync_NegativeStock_ThrowsArgumentException()
    {
        // Arrange
        var dto = new CreateProductDto
        {
            Name = "Invalid Product",
            Description = "Negative stock",
            Price = 10m,
            CategoryId = 1,
            StockQuantity = -5
        };

        // Act & Assert
        var exception =
            await Assert.ThrowsAsync<ArgumentException>(
                () => _productService.CreateAsync(dto));

        Assert.Equal(
            "Stock quantity cannot be negative.",
            exception.Message);
    }
    [Fact]
    public async Task GetByIdAsync_CacheHit_ReturnsCachedProductWithoutCallingRepository()
    {
        // Arrange
        var cachedProduct = new ProductDto
        {
            Id = 1,
            Name = "Cached Laptop",
            Description = "Cached product",
            Price = 899.99m,
            CategoryId = 1,
            StockQuantity = 20
        };

        _cacheServiceMock
            .Setup(cache =>
                cache.GetAsync<ProductDto>("product:1"))
            .ReturnsAsync(cachedProduct);

        // Act
        var result =
            await _productService.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Cached Laptop", result.Name);
        Assert.Equal(899.99m, result.Price);

        _productRepositoryMock.Verify(
            repository =>
                repository.GetByIdAsync(1),
            Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_CacheMiss_CallsRepositoryAndStoresProductInCache()
    {
        // Arrange
        var product = new Product
        {
            Id = 2,
            Name = "Keyboard",
            Description = "Test keyboard",
            Price = 49.99m,
            CategoryId = 1,
            StockQuantity = 15
        };

        _cacheServiceMock
            .Setup(cache =>
                cache.GetAsync<ProductDto>("product:2"))
            .ReturnsAsync((ProductDto?)null);

        _productRepositoryMock
            .Setup(repository =>
                repository.GetByIdAsync(2))
            .ReturnsAsync(product);

        // Act
        var result =
            await _productService.GetByIdAsync(2);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Id);
        Assert.Equal("Keyboard", result.Name);

        _productRepositoryMock.Verify(
            repository =>
                repository.GetByIdAsync(2),
            Times.Once);

        _cacheServiceMock.Verify(
            cache =>
                cache.SetAsync(
                    "product:2",
                    It.IsAny<ProductDto>(),
                    It.IsAny<TimeSpan>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ProductExists_RemovesProductFromCache()
    {
        // Arrange
        var product = new Product
        {
            Id = 3,
            Name = "Monitor",
            Description = "Test monitor",
            Price = 199.99m,
            CategoryId = 1,
            StockQuantity = 10
        };

        _productRepositoryMock
            .Setup(repository =>
                repository.GetByIdAsync(3))
            .ReturnsAsync(product);

        // Act
        var result =
            await _productService.DeleteAsync(3);

        // Assert
        Assert.True(result);

        _cacheServiceMock.Verify(
            cache =>
                cache.RemoveAsync("product:3"),
            Times.Once);

        _cacheServiceMock.Verify(
            cache =>
                cache.RemoveAsync("products:all"),
            Times.Once);
    }
}