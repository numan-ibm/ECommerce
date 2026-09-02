namespace ECommerce.Application.DTOs;

public class CartDto
{
    public int Id { get; set; }

    public decimal TotalAmount { get; set; }

    public List<CartItemDto> Items { get; set; } = new();
}