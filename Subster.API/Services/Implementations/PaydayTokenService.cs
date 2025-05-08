// Services/Implementations/PaydayTokenService.cs

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Subster.API.Clients;
using Subster.API.Exceptions;
using Subster.API.Services.Interfaces;
using Subster.DAL.Utilities;

namespace Subster.API.Services.Implementations;

public class PaydayTokenService : ITokenService
{
    private readonly IPaydayApiClient _paydayClient;
    private readonly IMemoryCache      _cache;
    private readonly EncryptionHelper _encryptionHelper;
    private const string CacheKeyPrefix = "payday_token_";

    public PaydayTokenService(
        IPaydayApiClient paydayClient,
        IMemoryCache cache,
        EncryptionHelper encryptionHelper)
    {
        _paydayClient      = paydayClient;
        _cache             = cache;
        _encryptionHelper  = encryptionHelper;
    }

    public async Task<string> GetTokenAsync(Guid trainerId, string clientId, string clientSecret)
    {
        var cacheKey = CacheKeyPrefix + trainerId;
        if (_cache.TryGetValue(cacheKey, out string protectedToken))
        {
            return _encryptionHelper.Unprotect(protectedToken);
        }

        var response = await _paydayClient.AuthenticateAsync(clientId, clientSecret);

        if (response?.AccessToken == null)
            throw new UnauthorizedException("Invalid client ID and/or client secret.");

        var expiresIn = TimeSpan.FromSeconds(response.ExpiresIn);
        _cache.Set(cacheKey,
                    _encryptionHelper.Protect(response.AccessToken),
                    expiresIn);

        return response.AccessToken;
    }
}
