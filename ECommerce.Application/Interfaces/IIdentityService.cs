namespace ECommerce.Application.Interfaces;

public interface IIdentityService
{
    Task<bool> RegisterAsync(string email, string password);

    Task<string?> LoginAsync(string email, string password);
}