namespace ECommerce.Application.Interfaces;

public interface IPaymentService
{
    Task<bool> ProcessPaymentAsync(
        int orderId,
        string userId);
}