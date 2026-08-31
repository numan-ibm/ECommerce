using ECommerce.Application.DTOs;
using ECommerce.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;

    public InventoryController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [HttpGet("{productId:int}")]
    public async Task<ActionResult<InventoryDto>> GetInventory(
        int productId)
    {
        var inventory =
            await _inventoryService.GetInventoryAsync(productId);

        if (inventory is null)
        {
            return NotFound();
        }

        return Ok(inventory);
    }

    [HttpPut("{productId:int}")]
    public async Task<IActionResult> UpdateStock(
        int productId,
        UpdateInventoryDto inventoryDto)
    {
        var updated = await _inventoryService.UpdateStockAsync(
            productId,
            inventoryDto);

        if (!updated)
        {
            return NotFound();
        }

        return NoContent();
    }
}