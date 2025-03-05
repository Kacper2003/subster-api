using Subster.API.Services.Interfaces;
using Subster.Models.InputModels;
using Subster.Models.Dtos;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Subster.API.Services.Implementations;

public class TaktikalAuthService : ITaktikalAuthService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public TaktikalAuthService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task<TaktikalAuthResult> AuthenticateAsync(AuthInputModel inputModel)
    {
        if (string.IsNullOrEmpty(inputModel.PhoneNumber) && string.IsNullOrEmpty(inputModel.Ssn))
        {
            return new TaktikalAuthResult { Authenticated = false, Error = "Either PhoneNumber or Ssn must be provided.", StatusCode = 400 };
        }

        var flowKey = _configuration["Taktikal:FlowKey"];
        if (string.IsNullOrEmpty(flowKey))
        {
            return new TaktikalAuthResult { Authenticated = false, Error = "FlowKey is missing from configuration.", StatusCode = 500 };
        }

        var startPayload = new
        {
            PhoneNumber = inputModel.PhoneNumber,
            Ssn = inputModel.Ssn,
            FlowKey = flowKey,
            AuthenticationContextType = !string.IsNullOrEmpty(inputModel.PhoneNumber) ? "Sim" : "App"
        };

        var startResult = await PostJsonAsync<StartAuthResponse>("https://onboardingdev.taktikal.is/api/auth/start", startPayload);

        if (!startResult.Success)
        {
            return new TaktikalAuthResult { Authenticated = false, Error = startResult.Error, StatusCode = startResult.StatusCode };
        }

        var authStart = startResult.Data;
        if (authStart == null || string.IsNullOrEmpty(authStart.AuthRequestId))
        {
            return new TaktikalAuthResult { Authenticated = false, Error = "Invalid JSON response from /api/auth/start.", StatusCode = 500 };
        }

        var pollPayload = new
        {
            authRequestId = authStart.AuthRequestId,
            FlowKey = flowKey,
            LookupType = "Name"
        };

        var timeout = TimeSpan.FromSeconds(180);
        var elapsed = TimeSpan.Zero;
        var pollingInterval = TimeSpan.FromSeconds(authStart.PollingInterval);

        while (elapsed < timeout)
        {
            await Task.Delay(pollingInterval);
            elapsed += pollingInterval;

            var pollResult = await PostJsonAsync<PollResponse>("https://onboardingdev.taktikal.is/api/auth/poll", pollPayload);

            if (!pollResult.Success)
            {
                if (pollResult.StatusCode == 403)
                {
                    return new TaktikalAuthResult { Authenticated = false, Error = "Authentication failed or timed out.", StatusCode = pollResult.StatusCode };
                }
                return new TaktikalAuthResult { Authenticated = false, Error = pollResult.Error, StatusCode = pollResult.StatusCode };
            }

            var pollData = pollResult.Data;
            if (pollData != null && !pollData.WaitingForUserInput)
            {
                return new TaktikalAuthResult { Authenticated = true, Customer = pollData.Customer, StatusCode = pollResult.StatusCode };
            }
        }

        return new TaktikalAuthResult { Authenticated = false, Error = "Authentication timed out.", StatusCode = 408 };
    }

    /// <summary>
    /// Helper method that sends a POST inputModel with JSON content and "Accept: application/json"
    /// </summary>
    private async Task<JsonApiResult<T>> PostJsonAsync<T>(string url, object payload)
    {
        var client = _httpClientFactory.CreateClient();
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(payload)
        };
        requestMessage.Headers.Accept.Clear();
        requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        
        var response = await client.SendAsync(requestMessage);
        var statusCode = (int)response.StatusCode;
        var rawContent = await response.Content.ReadAsStringAsync();
        
        if (!response.IsSuccessStatusCode)
        {
            return new JsonApiResult<T>(false, statusCode, rawContent, default);
        }
        
        try
        {
            var data = JsonSerializer.Deserialize<T>(rawContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return new JsonApiResult<T>(true, statusCode, null, data);
        }
        catch (JsonException ex)
        {
            var errorMsg = $"JSON Parse Error: {ex.Message}\nRaw Content:\n{rawContent}";
            return new JsonApiResult<T>(false, statusCode, errorMsg, default);
        }
    }
}

public record JsonApiResult<T>(bool Success, int StatusCode, string Error, T Data);