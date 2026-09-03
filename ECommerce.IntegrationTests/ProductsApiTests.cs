using System.Net;
using System.Net.Http.Json;
using ECommerce.Application.DTOs;
using Xunit;

namespace ECommerce.IntegrationTests;

public class ProductsApiTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ProductsApiTests(
        CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetProducts_WithAuthenticatedCustomer_ReturnsSuccess()
    {
        // Act
        var response =
            await _client.GetAsync("/api/products");

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_WithCustomerRole_ReturnsForbidden()
    {
        // Arrange
        var dto = new CreateProductDto
        {
            Name = "Integration Test Product",
            Description = "Test product",
            Price = 100m,
            CategoryId = 1,
            StockQuantity = 10
        };

        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/products",
                dto);

        // Assert
        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);
    }
}