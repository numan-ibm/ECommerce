using ECommerce.API.Hubs;
using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Moq;
using System.Security.Claims;
using System.Timers;
using Xunit;

namespace ECommerce.Tests.Hubs;

public class OrderHubTests
{
    [Fact]
    public async Task JoinOrderGroup_WithOwnedOrder_AddsConnectionToGroup()
    {
        var orderService = new Mock<IOrderService>();

        orderService
            .Setup(x => x.GetByIdAsync(10, "user-1"))
            .ReturnsAsync(new OrderDto
            {
                Id = 10,
                UserId = "user-1"
            });

        var hub = new OrderHub(orderService.Object);

        var groups = new Mock<IGroupManager>();

        var context = CreateHubContext(
            "connection-1",
            "user-1");

        hub.Context = context;
        hub.Groups = groups.Object;

        await hub.JoinOrderGroup(10);

        groups.Verify(
            x => x.AddToGroupAsync(
                "connection-1",
                "order-10",
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task JoinOrderGroup_WithOrderBelongingToAnotherUser_ThrowsHubException()
    {
        var orderService = new Mock<IOrderService>();

        orderService
            .Setup(x => x.GetByIdAsync(10, "user-1"))
            .ReturnsAsync((OrderDto?)null);

        var hub = new OrderHub(orderService.Object);

        var groups = new Mock<IGroupManager>();

        var context = CreateHubContext(
            "connection-1",
            "user-1");

        hub.Context = context;
        hub.Groups = groups.Object;

        var exception = await Assert.ThrowsAsync<HubException>(
            () => hub.JoinOrderGroup(10));

        Assert.Equal(
            "You are not authorized to access this order.",
            exception.Message);

        groups.Verify(
            x => x.AddToGroupAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task JoinOrderGroup_WithoutUserId_ThrowsHubException()
    {
        var orderService = new Mock<IOrderService>();

        var hub = new OrderHub(orderService.Object);

        var groups = new Mock<IGroupManager>();

        var context = CreateHubContext(
            "connection-1",
            null);

        hub.Context = context;
        hub.Groups = groups.Object;

        var exception = await Assert.ThrowsAsync<HubException>(
            () => hub.JoinOrderGroup(10));

        Assert.Equal(
            "User is not authenticated.",
            exception.Message);

        orderService.Verify(
            x => x.GetByIdAsync(
                It.IsAny<int>(),
                It.IsAny<string>()),
            Times.Never);

        groups.Verify(
            x => x.AddToGroupAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task LeaveOrderGroup_RemovesConnectionFromGroup()
    {
        var orderService = new Mock<IOrderService>();

        var hub = new OrderHub(orderService.Object);

        var groups = new Mock<IGroupManager>();

        var context = CreateHubContext(
            "connection-1",
            "user-1");

        hub.Context = context;
        hub.Groups = groups.Object;

        await hub.LeaveOrderGroup(10);

        groups.Verify(
            x => x.RemoveFromGroupAsync(
                "connection-1",
                "order-10",
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static HubCallerContext CreateHubContext(
        string connectionId,
        string? userId)
    {
        var claims = new List<Claim>();

        if (userId != null)
        {
            claims.Add(
                new Claim(
                    ClaimTypes.NameIdentifier,
                    userId));
        }

        var identity = new ClaimsIdentity(
            claims,
            "TestAuthentication");

        var principal = new ClaimsPrincipal(identity);

        var context = new Mock<HubCallerContext>();

        context
            .SetupGet(x => x.ConnectionId)
            .Returns(connectionId);

        context
            .SetupGet(x => x.User)
            .Returns(principal);

        return context.Object;
    }
}