namespace Subster.API.Services.Interfaces;

public interface ITokenService
{
    Task<string?> GetTokenAsync(int trainerId, string clientId, string clientSecret);
}

