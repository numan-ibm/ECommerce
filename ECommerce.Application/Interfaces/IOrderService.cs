using ECommerce.Application.DTOs;

namespace ECommerce.Application.Interfaces;

public interface IOrderService
{
    // Customer operations
    Task<OrderDto?> GetByIdAsync(
        int id,
        string userId);

    Task<IEnumerable<OrderDto>> GetMyOrdersAsync(
        string userId);

    Task<OrderDto> CreateOrderAsync(
        CreateOrderDto dto,
        string userId);

    // Admin operations
    Task<IEnumerable<OrderDto>> GetAllOrdersAsync();

    Task<OrderDto?> GetByIdForAdminAsync(
        int id);

    Task<OrderDto?> UpdateStatusAsync(
        int id,
        string status);
}