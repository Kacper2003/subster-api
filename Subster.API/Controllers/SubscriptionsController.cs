using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Subster.API.Services.Interfaces;
using Subster.Models.InputModels;


[ApiController]
[Route("api/[controller]")]
public class SubscriptionsController : ControllerBase
{
    private readonly ITaktikalAuthService _taktikalAuthService;

    public SubscriptionsController(ITaktikalAuthService taktikalAuthService)
    {
        _taktikalAuthService = taktikalAuthService;
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

        // Create subscription
        return Ok(new
        {
            Subscription = new
            {
                SubscriptionId = Guid.NewGuid(),
                Customer = authResult.Customer
            }
        });
    }
}