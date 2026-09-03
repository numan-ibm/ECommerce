namespace ECommerce.Application.Interfaces;

public interface IOrderNotificationService
{
    Task NotifyOrderStatusChangedAsync(
        int orderId,
        string status);
}