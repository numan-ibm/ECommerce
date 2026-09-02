using ECommerce.Application.DTOs;

namespace ECommerce.Application.Interfaces;

public interface ICartService
{
    Task<CartDto> GetCartAsync(string userId);

    Task<CartDto> AddToCartAsync(
        string userId,
        AddToCartDto dto);

    Task<CartDto> UpdateCartItemAsync(
        string userId,
        int productId,
        int quantity);

    Task RemoveFromCartAsync(
        string userId,
        int productId);

    Task ClearCartAsync(string userId);

    Task<OrderDto> CheckoutAsync(string userId);
}