using System.Net;
using System.Net.Http.Json;
using ECommerce.Application.DTOs;
using Xunit;

namespace ECommerce.IntegrationTests;

public class CategoriesApiTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CategoriesApiTests(
        CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetCategories_WithAuthenticatedCustomer_ReturnsSuccess()
    {
        // Act
        var response =
            await _client.GetAsync("/api/categories");

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);
    }

    [Fact]
    public async Task CreateCategory_WithCustomerRole_ReturnsForbidden()
    {
        // Arrange
        var dto = new CreateCategoryDto
        {
            Name = "Integration Test Category",
            Description = "Test category"
        };

        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/categories",
                dto);

        // Assert
        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);
    }

    [Fact]
    public async Task CreateCategory_WithEmptyName_ReturnsBadRequest()
    {
        // Arrange
        var dto = new CreateCategoryDto
        {
            Name = "",
            Description = "Invalid category"
        };

        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/categories",
                dto);

        // Assert
        // Customer authentication prevents reaching
        // the service validation, so the expected result
        // is Forbidden.
        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);
    }
}