using ECommerce.API.Hubs;
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace ECommerce.API.Services;

public class OrderNotificationService : IOrderNotificationService
{
    private readonly IHubContext<OrderHub> _hubContext;

    public OrderNotificationService(
        IHubContext<OrderHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyOrderStatusChangedAsync(
        int orderId,
        string status)
    {
        await _hubContext
            .Clients
            .Group($"order-{orderId}")
            .SendAsync(
                "OrderStatusChanged",
                new
                {
                    OrderId = orderId,
                    Status = status
                });
    }
}