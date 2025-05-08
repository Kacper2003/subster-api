namespace Subster.API.Services.Interfaces;

public interface ITokenService
{
    Task<string> GetTokenAsync(Guid trainerId, string clientId, string clientSecret);
    Task<bool> ValidateCredentialsAsync(string clientId, string clientSecret);
}

