using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Subster.API.Services.Interfaces;
using Subster.Models.InputModels;


[ApiController]
[Route("api/[controller]")]
public class SubscriptionsController : ControllerBase
{
    private readonly ITaktikalAuthService _taktikalAuthService;
    private readonly ISubscriptionService _subscriptionService;

    public SubscriptionsController(ITaktikalAuthService taktikalAuthService, ISubscriptionService subscriptionService)
    {
        _taktikalAuthService = taktikalAuthService;
        _subscriptionService = subscriptionService;
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateSubscription([FromBody] AuthInputModel inputModel)
    {
        var authResult = await _taktikalAuthService.AuthenticateAsync(inputModel);
        if (!authResult.Authenticated)
        {
            return BadRequest(authResult);
        }

        // Get the user's SSN from the token
        var ssn = User.Claims.FirstOrDefault(c => c.Type == "Ssn")?.Value;
        if (ssn == null)
        {
            return BadRequest("SSN not found in token");
        }

        await _subscriptionService.CreateSubscriptionAsync(new SubscriptionInputModel
        {
            UserSsn = ssn,
            ClientName = authResult.Customer.Name,
            ClientSsn = authResult.Customer.Ssn
        });

        return Created();
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetSubscriptions()
    {
        // Get the user's SSN from the token
        var ssn = User.Claims.FirstOrDefault(c => c.Type == "Ssn")?.Value;
        if (ssn == null)
        {
            return BadRequest("SSN not found in token");
        }

        var subscriptions = await _subscriptionService.GetSubscriptionsAsync(ssn);
        return Ok(subscriptions);
    }
}