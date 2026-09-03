using System.Net;
using System.Net.Http.Json;
using ECommerce.Application.DTOs;
using Xunit;

namespace ECommerce.IntegrationTests;

public class OrdersApiTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public OrdersApiTests(
        CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetMyOrders_WithAuthenticatedCustomer_ReturnsSuccess()
    {
        // Act
        var response =
            await _client.GetAsync("/api/orders");

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);
    }

    [Fact]
    public async Task GetOrderById_WithNonExistingOrder_ReturnsNotFound()
    {
        // Act
        var response =
            await _client.GetAsync("/api/orders/999999");

        // Assert
        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    [Fact]
    public async Task CreateOrder_WithEmptyItems_ReturnsBadRequest()
    {
        // Arrange
        var dto = new CreateOrderDto
        {
            Items = new List<CreateOrderItemDto>()
        };

        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/orders",
                dto);

        // Assert
        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }
}