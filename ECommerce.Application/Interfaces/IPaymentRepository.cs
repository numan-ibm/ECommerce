using ECommerce.Domain.Models;

namespace ECommerce.Application.Interfaces;

public interface IPaymentRepository
{
    Task<Payment?> GetByOrderIdAsync(int orderId);

    Task<Payment> AddAsync(Payment payment);

    Task SaveChangesAsync();
}