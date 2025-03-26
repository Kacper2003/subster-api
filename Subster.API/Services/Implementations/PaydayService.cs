using Microsoft.Extensions.Caching.Memory;
using Subster.API.Services.Interfaces;
using Subster.DAL.Interfaces;
using Subster.DAL.Utilities;
using Subster.Models.Dtos.Payday;

namespace Subster.API.Services.Implementations;

public class PaydayService : IPaydayService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ITrainerRepository _trainerRepository;
    private readonly IMemoryCache _cache;
    private readonly EncryptionHelper _encryptionHelper;
    private const string CacheKeyPrefix = "payday_token_";

    public PaydayService(IHttpClientFactory httpClientFactory, ITrainerRepository trainerRepository, IMemoryCache cache, EncryptionHelper encryptionHelper)
    {
        _httpClientFactory = httpClientFactory;
        _trainerRepository = trainerRepository;
        _cache = cache;
        _encryptionHelper = encryptionHelper;
    }

    public async Task<bool> UpdateCredentials(string ssn, string clientId, string clientSecret)
    {
        var trainer = await _trainerRepository.GetTrainerBySsnAsync(ssn);
        if (trainer == null)
        {
            return false;
        }

        // check if the client ID and client secret are valid
        var tokenResponse = await RequestPaydayTokenAsync(clientId, clientSecret);
        if (tokenResponse == null)
        {
            return false;
        }

        await _trainerRepository.UpdatePaydayCredentialsAsync(trainer.Id, clientId, clientSecret);

        return true;
    }

    public async Task<bool> DeleteCredentials(string ssn)
    {
        var trainer = await _trainerRepository.GetTrainerBySsnAsync(ssn);
        if (trainer == null)
        {
            return false;
        }

        await _trainerRepository.UpdatePaydayCredentialsAsync(trainer.Id, null, null);

        _cache.Remove(CacheKeyPrefix + trainer.Id);

        return true;
    }

    public async Task<string?> GetAccessToken(string ssn)
    {
        var trainer = await _trainerRepository.GetTrainerEntityBySsnAsync(ssn);
        if (trainer == null)
        {
            return null;
        }

        if (_cache.TryGetValue(CacheKeyPrefix + trainer.Id, out string? cachedToken) && cachedToken != null)
        {
            return _encryptionHelper.Unprotect(cachedToken);
        }

        if (!string.IsNullOrEmpty(trainer.PaydayClientId) && !string.IsNullOrEmpty(trainer.PaydayClientSecret))
        {
            var tokenResponse = await RequestPaydayTokenAsync(trainer.PaydayClientId, trainer.PaydayClientSecret);
            if (tokenResponse == null)
            {
                return null;
            }

            _cache.Set(CacheKeyPrefix + trainer.Id, _encryptionHelper.Protect(tokenResponse.AccessToken), TimeSpan.FromSeconds(tokenResponse.ExpiresIn));

            return tokenResponse.AccessToken;
        }

        return null;
    }

    private async Task<PaydayTokenResponse?> RequestPaydayTokenAsync(string clientId, string clientSecret)
    {
        var httpClient = _httpClientFactory.CreateClient();
        var response = await httpClient.PostAsJsonAsync("https://api.test.payday.is/auth/token", new
        {
            clientId,
            clientSecret
        });
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }
        var tokenResponse = await response.Content.ReadFromJsonAsync<PaydayTokenResponse>();
        return tokenResponse;
    }
}