// Services/Implementations/PaydayTokenService.cs

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Subster.API.Clients;
using Subster.API.Exceptions;
using Subster.API.Services.Interfaces;
using Subster.DAL.Utilities;

namespace Subster.API.Services.Implementations;

public class PaydayTokenService(
	IPaydayApiClient paydayClient,
	IMemoryCache cache,
	EncryptionHelper encryptionHelper) : ITokenService
{
    private readonly IPaydayApiClient _paydayClient = paydayClient;
    private readonly IMemoryCache _cache = cache;
    private readonly EncryptionHelper _encryptionHelper = encryptionHelper;
    private const string CacheKeyPrefix = "payday_token_";

	public async Task<string> GetTokenAsync(Guid trainerId, string clientId, string clientSecret)
    {
        // First, check if the token is already cached
        var cacheKey = CacheKeyPrefix + trainerId;
        if (_cache.TryGetValue(cacheKey, out string? protectedToken) && !string.IsNullOrEmpty(protectedToken))
        {
            return _encryptionHelper.Unprotect(protectedToken);
        }

        // If not cached, authenticate with the Payday API
		Models.Dtos.Payday.PaydayTokenResponse response = await _paydayClient.AuthenticateAsync(clientId, clientSecret) 
            ?? throw new UnauthorizedException("Failed to authenticate with Payday API.");

        if (string.IsNullOrEmpty(response.AccessToken))
            throw new Exception("Failed to retrieve access token from Payday API.");

        // Cache the token with an expiration time
        var expiresIn = TimeSpan.FromSeconds(response.ExpiresIn);
        _cache.Set(cacheKey,
                    _encryptionHelper.Protect(response.AccessToken),
                    expiresIn);

        return response.AccessToken;
    }

    // This method is used to validate the Payday credentials of the trainer
    public async Task<bool> ValidateCredentialsAsync(string clientId, string clientSecret)
    {
		Models.Dtos.Payday.PaydayTokenResponse? response = await _paydayClient.AuthenticateAsync(clientId, clientSecret);
        return response != null;
    }

    public void RemoveToken(Guid trainerId)
    {
        var cacheKey = CacheKeyPrefix + trainerId;
        _cache.Remove(cacheKey);
    }
}
