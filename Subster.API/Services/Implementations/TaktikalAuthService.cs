using Subster.API.Clients;
using Subster.API.Services.Interfaces;
using Subster.Models.InputModels;
using Subster.Models.ResponseModels;

namespace Subster.API.Services.Implementations;

public class TaktikalAuthService : ITaktikalAuthService
{
    private readonly ITaktikalApiClient _client;
    private readonly IConfiguration      _config;

    public TaktikalAuthService(ITaktikalApiClient client, IConfiguration config)
    {
        _client = client;
        _config = config;
    }

    public async Task<EndAuthResponseModel> AuthenticateAsync(AuthInputModel inputModel)
    {
        // 1) Validate input
        if (string.IsNullOrEmpty(inputModel.PhoneNumber)
         && string.IsNullOrEmpty(inputModel.Ssn))
        {
            return new EndAuthResponseModel
            {
                Authenticated = false,
                Error         = "Either PhoneNumber or Ssn must be provided.",
                StatusCode    = 400
            };
        }

        var flowKey = _config["Taktikal:FlowKey"];
        if (string.IsNullOrEmpty(flowKey))
        {
            return new EndAuthResponseModel
            {
                Authenticated = false,
                Error         = "FlowKey is missing from configuration.",
                StatusCode    = 500
            };
        }

        var startDto = new StartAuthInputModel
        {
            PhoneNumber               = inputModel.PhoneNumber,
            Ssn                       = inputModel.Ssn,
            FlowKey                   = flowKey,
            AuthenticationContextType = 
               !string.IsNullOrEmpty(inputModel.PhoneNumber) ? "Sim" : "App"
        };
        
        var start = await _client.StartAsync(startDto);
        if (start == null)
        {
            return new EndAuthResponseModel
            {
                Authenticated = false,
                Error         = "Failed to call /auth/start",
                StatusCode    = 502
            };
        }

        var timeout         = TimeSpan.FromSeconds(180);
        var expiry          = DateTime.UtcNow + timeout;
        var pollingInterval = TimeSpan.FromSeconds(start.PollingInterval);

        while (DateTime.UtcNow < expiry)
        {
            await Task.Delay(pollingInterval);

            var pollDto = new PollAuthInputModel
            {
                AuthRequestId = start.AuthRequestId,
                FlowKey       = flowKey,
                LookupType    = "Name"
            };
            var poll = await _client.PollAsync(pollDto);

            if (poll == null)
            {
                return new EndAuthResponseModel
                {
                    Authenticated = false,
                    Error         = "Failed to call /auth/poll",
                    StatusCode    = 502
                };
            }

            if (!poll.WaitingForUserInput)
            {
                return new EndAuthResponseModel
                {
                    Authenticated = true,
                    Customer      = poll.Customer,
                    StatusCode    = 200
                };
            }
        }

        return new EndAuthResponseModel
        {
            Authenticated = false,
            Error         = "Authentication timed out.",
            StatusCode    = 408
        };
    }
}
