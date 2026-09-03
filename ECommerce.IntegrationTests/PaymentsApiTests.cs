using System.Net;
using Xunit;

namespace ECommerce.IntegrationTests;

public class PaymentsApiTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public PaymentsApiTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ProcessPayment_WithNonExistingOrder_ReturnsBadRequest()
    {
        var response = await _client.PostAsync(
            "/api/payments/999999",
            null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetPayment_WithNonExistingOrder_ReturnsNotFound()
    {
        var response = await _client.GetAsync(
            "/api/payments/order/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}