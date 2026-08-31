using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;

namespace ECommerce.Application.Services;

public class InventoryService : IInventoryService
{
    private readonly IProductRepository _productRepository;

    public InventoryService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<InventoryDto?> GetInventoryAsync(int productId)
    {
        var product = await _productRepository.GetByIdAsync(productId);

        if (product is null)
        {
            return null;
        }

        return new InventoryDto
        {
            ProductId = product.Id,
            ProductName = product.Name,
            StockQuantity = product.StockQuantity,
            IsInStock = product.StockQuantity > 0
        };
    }

    public async Task<bool> UpdateStockAsync(
        int productId,
        UpdateInventoryDto inventoryDto)
    {
        if (inventoryDto.StockQuantity < 0)
        {
            return false;
        }

        var product = await _productRepository.GetByIdAsync(productId);

        if (product is null)
        {
            return false;
        }

        product.StockQuantity = inventoryDto.StockQuantity;

        await _productRepository.UpdateAsync(product);

        return true;
    }
}