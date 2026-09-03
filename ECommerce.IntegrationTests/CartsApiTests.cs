using System.Net;
using System.Net.Http.Json;
using ECommerce.Application.DTOs;
using Xunit;

namespace ECommerce.IntegrationTests;

public class CartsApiTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CartsApiTests(
        CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetCart_WithAuthenticatedCustomer_ReturnsSuccess()
    {
        // Act
        var response =
            await _client.GetAsync("/api/carts");

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);
    }

    [Fact]
    public async Task AddToCart_WithInvalidQuantity_ReturnsBadRequest()
    {
        // Arrange
        var dto = new AddToCartDto
        {
            ProductId = 1,
            Quantity = 0
        };

        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/carts/items",
                dto);

        // Assert
        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    [Fact]
    public async Task AddToCart_WithMissingProduct_ReturnsBadRequest()
    {
        // Arrange
        var dto = new AddToCartDto
        {
            ProductId = 999999,
            Quantity = 1
        };

        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/carts/items",
                dto);

        // Assert
        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    [Fact]
    public async Task UpdateCartItem_WithInvalidQuantity_ReturnsBadRequest()
    {
        // Act
        var response =
            await _client.PutAsJsonAsync(
                "/api/carts/items/1",
                0);

        // Assert
        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    [Fact]
    public async Task RemoveFromCart_WithAuthenticatedCustomer_ReturnsNoContent()
    {
        // Act
        var response =
            await _client.DeleteAsync(
                "/api/carts/items/999999");

        // Assert
        Assert.Equal(
            HttpStatusCode.NoContent,
            response.StatusCode);
    }

    [Fact]
    public async Task ClearCart_WithAuthenticatedCustomer_ReturnsNoContent()
    {
        // Act
        var response =
            await _client.DeleteAsync("/api/carts");

        // Assert
        Assert.Equal(
            HttpStatusCode.NoContent,
            response.StatusCode);
    }

    [Fact]
    public async Task Checkout_WithEmptyCart_ReturnsBadRequest()
    {
        // Arrange
        // The integration test user starts with an empty cart
        // unless another test has populated it.

        // Act
        var response =
            await _client.PostAsync(
                "/api/carts/checkout",
                null);

        // Assert
        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }
}