namespace ECommerce.Application.Interfaces;

public interface IOrderBackgroundProcessor
{
    Task ProcessOrderAsync(
        int orderId,
        CancellationToken cancellationToken);
}