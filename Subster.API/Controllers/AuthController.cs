using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Subster.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public AuthController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> AuthenticateUser([FromBody] AuthRequestDto request)
        {
            // 1. Validate input
            if (string.IsNullOrEmpty(request.PhoneNumber) && string.IsNullOrEmpty(request.Ssn))
            {
                return BadRequest("Either PhoneNumber or Ssn must be provided.");
            }

            // 2. Get FlowKey from config
            var flowKey = _configuration["Taktikal:FlowKey"];
            if (string.IsNullOrEmpty(flowKey))
            {
                return StatusCode(500, "FlowKey is missing from configuration.");
            }

            // 3. Prepare start-auth payload
            var startPayload = new
            {
                PhoneNumber = request.PhoneNumber,
                Ssn = request.Ssn,
                FlowKey = flowKey,
                // "Sim" if PhoneNumber is provided, otherwise "App"
                AuthenticationContextType = !string.IsNullOrEmpty(request.PhoneNumber) ? "Sim" : "App"
            };

            // 4. Call /api/auth/start, expecting JSON
            var startResponse = await PostJsonAsync<StartAuthResponse>(
                "https://onboardingdev.taktikal.is/api/auth/start",
                startPayload
            );
            if (!startResponse.Success)
            {
                // If Taktikal returned a non-2xx code or invalid JSON, handle it
                return StatusCode(startResponse.StatusCode, new { Error = startResponse.Error });
            }

            var authStart = startResponse.Data;
            if (authStart == null || string.IsNullOrEmpty(authStart.AuthRequestId))
            {
                return StatusCode(500, "Invalid JSON response from /api/auth/start.");
            }

            // 5. Prepare polling payload with "Name" lookup
            var pollPayload = new
            {
                authRequestId = authStart.AuthRequestId,
                FlowKey = flowKey,
                LookupType = "Name" // "NameAddress", "NameAddressFamily" if you need more data
            };

            // 6. Poll for up to 180 seconds
            var timeout = TimeSpan.FromSeconds(180);
            var elapsed = TimeSpan.Zero;
            var pollingInterval = TimeSpan.FromSeconds(authStart.PollingInterval);

            while (elapsed < timeout)
            {
                // Wait for the specified polling interval
                await Task.Delay(pollingInterval);
                elapsed += pollingInterval;

                // Call /api/auth/poll
                var pollResponse = await PostJsonAsync<PollResponse>(
                    "https://onboardingdev.taktikal.is/api/auth/poll",
                    pollPayload
                );

                if (!pollResponse.Success)
                {
                    // 403 => user canceled or timed out on Taktikal side
                    if (pollResponse.StatusCode == 403)
                    {
                        return Unauthorized(new { Authenticated = false, Message = "Authentication failed or timed out." });
                    }
                    // Other error codes
                    return StatusCode(pollResponse.StatusCode, new { Error = pollResponse.Error });
                }

                var pollResult = pollResponse.Data;
                // If user is no longer waiting for input => success!
                if (pollResult != null && !pollResult.WaitingForUserInput)
                {
                    // Return the entire Customer object (Name, Ssn, etc.)
                    return Ok(new
                    {
                        Authenticated = true,
                        Customer = pollResult.Customer
                    });
                }
            }

            // If we reach here, we timed out after 3 minutes
            return StatusCode(408, new { Authenticated = false, Message = "Authentication timed out." });
        }

        /// <summary>
        /// Helper method to POST JSON with Accept: application/json
        /// and deserialize the JSON response into T.
        /// </summary>
        private async Task<JsonApiResult<T>> PostJsonAsync<T>(string url, object payload)
        {
            var client = _httpClientFactory.CreateClient();

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = JsonContent.Create(payload)
            };
            // Force JSON
            requestMessage.Headers.Accept.Clear();
            requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await client.SendAsync(requestMessage);
            var statusCode = (int)response.StatusCode;
            var rawContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                // Return raw content in case Taktikal sends an error message
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

    /// <summary>
    /// Generic container for success/failure info + parsed T
    /// </summary>
    public record JsonApiResult<T>(bool Success, int StatusCode, string Error, T Data);

    // Incoming request
    public class AuthRequestDto
    {
        public string? PhoneNumber { get; set; }
        public string? Ssn { get; set; }
    }

    // /api/auth/start response
    public class StartAuthResponse
    {
        public string AuthRequestId { get; set; }
        public string VerificationCode { get; set; }
        public int PollingInterval { get; set; }
    }

    // /api/auth/poll response (with customer data)
    public class PollResponse
    {
        public bool WaitingForUserInput { get; set; }
        public string StatusMessage { get; set; }
        public CustomerDto Customer { get; set; }
    }

    // Customer data from "Name" lookup (partial fields).
    // If you use "NameAddress" or "NameAddressFamily", they return more fields.
    public class CustomerDto
    {
        public string Name { get; set; }
        public string Ssn { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string PostalCode { get; set; }
        public string City { get; set; }
        public string Token { get; set; }
        // If you use "NameAddressFamily", you'd get additional fields in "meta", e.g., Family info
        // public Dictionary<string, string> Meta { get; set; }
    }
}
