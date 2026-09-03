using System.Net;
using System.Net.Http.Json;
using ECommerce.Application.DTOs;
using Xunit;

namespace ECommerce.IntegrationTests;

public class AuthApiTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthApiTests(
        CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_WithValidUser_ReturnsSuccess()
    {
        // Arrange
        var email =
            $"integration-{Guid.NewGuid()}@example.com";

        var dto = new RegisterDto
        {
            Email = email,
            Password = "Test@12345",
            ConfirmPassword = "Test@12345"
        };

        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/auth/register",
                dto);

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsBadRequest()
    {
        // Arrange
        var email =
            $"duplicate-{Guid.NewGuid()}@example.com";

        var dto = new RegisterDto
        {
            Email = email,
            Password = "Test@12345",
            ConfirmPassword = "Test@12345"
        };

        // First registration
        var firstResponse =
            await _client.PostAsJsonAsync(
                "/api/auth/register",
                dto);

        Assert.Equal(
            HttpStatusCode.OK,
            firstResponse.StatusCode);

        // Act
        var secondResponse =
            await _client.PostAsJsonAsync(
                "/api/auth/register",
                dto);

        // Assert
        Assert.Equal(
            HttpStatusCode.BadRequest,
            secondResponse.StatusCode);
    }

    [Fact]
    public async Task Register_WithDifferentPasswords_ReturnsBadRequest()
    {
        // Arrange
        var dto = new RegisterDto
        {
            Email =
                $"password-{Guid.NewGuid()}@example.com",
            Password = "Test@12345",
            ConfirmPassword = "Different@12345"
        };

        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/auth/register",
                dto);

        // Assert
        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var dto = new LoginDto
        {
            Email =
                $"nonexistent-{Guid.NewGuid()}@example.com",
            Password = "Wrong@12345"
        };

        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/auth/login",
                dto);

        // Assert
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }
}