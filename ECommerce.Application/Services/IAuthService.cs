using ECommerce.Application.DTOs;

namespace ECommerce.Application.Services;

public interface IAuthService
{
    Task<bool> RegisterAsync(RegisterDto registerDto);

    Task<string?> LoginAsync(LoginDto loginDto);
}