using ECommerce.Application.DTOs;

namespace ECommerce.Application.Services;

public interface IInventoryService
{
    Task<InventoryDto?> GetInventoryAsync(int productId);

    Task<bool> UpdateStockAsync(
        int productId,
        UpdateInventoryDto inventoryDto);
}