using ECommerce.Domain.Models;

namespace ECommerce.Application.Interfaces;

public interface ICartRepository
{
    Task<Cart?> GetByUserIdAsync(string userId);

    Task<Cart?> GetByIdAsync(int cartId);
    Task<Cart?> GetForCheckoutAsync(string userId);

    Task<Cart> AddAsync(Cart cart);

    Task<CartItem> AddItemAsync(CartItem item);

    Task<CartItem?> GetItemAsync(
        int cartId,
        int productId);

    Task RemoveItemAsync(CartItem item);

    Task SaveChangesAsync();
}