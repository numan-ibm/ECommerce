using ECommerce.Domain.Models;

namespace ECommerce.Application.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(int id);

    Task<IEnumerable<Order>> GetByUserIdAsync(string userId);

    Task<IEnumerable<Order>> GetAllAsync();

    Task<Order> AddAsync(Order order);

    Task SaveChangesAsync();
}