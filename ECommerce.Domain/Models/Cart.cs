namespace ECommerce.Domain.Models;

public class Cart
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; }

    public List<CartItem> Items { get; set; } = new();
}