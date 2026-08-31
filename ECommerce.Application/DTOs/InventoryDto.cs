namespace ECommerce.Application.DTOs;

public class InventoryDto
{
    public int ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public int StockQuantity { get; set; }

    public bool IsInStock { get; set; }
}