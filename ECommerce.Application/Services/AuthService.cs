using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;

namespace ECommerce.Application.Services;

public class AuthService : IAuthService
{
    private readonly IIdentityService _identityService;

    public AuthService(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<bool> RegisterAsync(RegisterDto registerDto)
    {
        if (registerDto.Password != registerDto.ConfirmPassword)
        {
            return false;
        }

        return await _identityService.RegisterAsync(
            registerDto.Email,
            registerDto.Password);
    }

    public async Task<string?> LoginAsync(LoginDto loginDto)
    {
        return await _identityService.LoginAsync(
            loginDto.Email,
            loginDto.Password);
    }
}