using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace ECommerce.IntegrationTests;

public class AdminOrdersApiTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AdminOrdersApiTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAllOrders_WithCustomerRole_ReturnsForbidden()
    {
        var response = await _client.GetAsync(
            "/api/orders/admin");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetOrderByIdForAdmin_WithCustomerRole_ReturnsForbidden()
    {
        var response = await _client.GetAsync(
            "/api/orders/admin/999999");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateOrderStatus_WithCustomerRole_ReturnsForbidden()
    {
        var response = await _client.PutAsJsonAsync(
            "/api/orders/admin/999999/status",
            new { status = "Shipped" });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}