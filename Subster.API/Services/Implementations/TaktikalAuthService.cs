using Subster.API.Clients;
using Subster.API.Services.Interfaces;
using Subster.Models.InputModels;
using Subster.Models.ResponseModels;

namespace Subster.API.Services.Implementations;

public class TaktikalAuthService(ITaktikalApiClient client, IConfiguration config) : ITaktikalAuthService
{
    private readonly ITaktikalApiClient _client = client;
    private readonly IConfiguration _config = config;

    // Method to authenticate a user
	public async Task<EndAuthResponseModel> AuthenticateAsync(AuthInputModel inputModel)
    {
        // Needs to have either PhoneNumber or Ssn
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

        // Get the key to be able to call the Taktikal API
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

        // Call the Taktikal API to start the authentication process
		StartAuthResponseModel? start = await _client.StartAsync(startDto);
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
		DateTime expiry          = DateTime.UtcNow + timeout;
        var pollingInterval = TimeSpan.FromSeconds(start.PollingInterval);

        // Poll each (pollingInterval) seconds until the authentication is complete or timeout
        while (DateTime.UtcNow < expiry)
        {
            await Task.Delay(pollingInterval);

            var pollDto = new PollAuthInputModel
            {
                AuthRequestId = start.AuthRequestId,
                FlowKey       = flowKey,
                LookupType    = "Name"
            };
			PollResponseModel? poll = await _client.PollAsync(pollDto);

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
