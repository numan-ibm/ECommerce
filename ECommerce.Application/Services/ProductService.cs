using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<IEnumerable<ProductDto>> GetAllAsync()
    {
        var products = await _productRepository.GetAllAsync();

        return products.Select(MapToDto);
    }

    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);

        return product is null ? null : MapToDto(product);
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto productDto)
    {
        var product = new Product
        {
            Name = productDto.Name,
            Description = productDto.Description,
            Price = productDto.Price,
            CategoryId = productDto.CategoryId,
            StockQuantity = productDto.StockQuantity,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createdProduct = await _productRepository.AddAsync(product);

        return MapToDto(createdProduct);
    }

    public async Task<bool> UpdateAsync(
        int id,
        CreateProductDto productDto)
    {
        var product = await _productRepository.GetByIdAsync(id);

        if (product is null)
        {
            return false;
        }

        product.Name = productDto.Name;
        product.Description = productDto.Description;
        product.Price = productDto.Price;
        product.CategoryId = productDto.CategoryId;
        product.StockQuantity = productDto.StockQuantity;
        product.UpdatedAt = DateTime.UtcNow;

        await _productRepository.UpdateAsync(product);

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);

        if (product is null)
        {
            return false;
        }

        await _productRepository.DeleteAsync(id);

        return true;
    }

    private static ProductDto MapToDto(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            CategoryId = product.CategoryId,
            StockQuantity = product.StockQuantity
        };
    }
}