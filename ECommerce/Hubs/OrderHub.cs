using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace ECommerce.API.Hubs;

[Authorize]
public class OrderHub : Hub
{
    private readonly IOrderService _orderService;

    public OrderHub(IOrderService orderService)
    {
        _orderService = orderService;
    }

    public async Task JoinOrderGroup(int orderId)
    {
        var userId =
            Context.User?.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            throw new HubException(
                "User is not authenticated.");
        }

        var order =
            await _orderService.GetByIdAsync(
                orderId,
                userId);

        if (order == null)
        {
            throw new HubException(
                "You are not authorized to access this order.");
        }

        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            $"order-{orderId}");
    }

    public async Task LeaveOrderGroup(int orderId)
    {
        await Groups.RemoveFromGroupAsync(
            Context.ConnectionId,
            $"order-{orderId}");
    }
}