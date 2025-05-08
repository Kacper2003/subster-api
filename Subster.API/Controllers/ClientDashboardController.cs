using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Subster.API.Services.Interfaces;
using Subster.Models.Dtos;
using Subster.Models;

namespace Subster.API.Controllers;

[ApiController]
[Route("api/client")]
[Produces("application/json")]
[Consumes("application/json")]
public class ClientDashboardController : ControllerBase
{
    private readonly IClientDashboardService _clientDashboardService;

    public ClientDashboardController(IClientDashboardService clientDashboardService)
    {
        _clientDashboardService = clientDashboardService;
    }

    [HttpGet("subscriptions")]
    [Authorize(Roles = "Client")]
    [ProducesResponseType(typeof(IEnumerable<SubscriptionDto>), 200)]
    public async Task<IActionResult> GetSubscriptions()
    {
        var clientSsn = User.Claims.FirstOrDefault(c => c.Type == "Ssn")?.Value;
        if (string.IsNullOrEmpty(clientSsn))
        {
            return Unauthorized("SSN not found in token");
        }

        var subscriptions = await _clientDashboardService.GetSubscriptionsByClientSsnAsync(clientSsn);
        return Ok(subscriptions);
    }
    
    [HttpDelete("subscriptions/{id:guid}")]
    [Authorize(Roles = "Client")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiError), 409)]
    public async Task<IActionResult> DeactivateSubscription(Guid id)
    {
        var clientSsn = User.Claims.FirstOrDefault(c => c.Type == "Ssn")?.Value;
        if (string.IsNullOrEmpty(clientSsn))
        {
            return Unauthorized("SSN not found in token");
        }

        await _clientDashboardService.DeactivateSubscriptionAsync(clientSsn, id);
        return NoContent();
    }

    [HttpGet("trainers")]
    [ProducesResponseType(typeof(IEnumerable<TrainerDto>), 200)]
    public async Task<IActionResult> GetTrainers()
    {
        var trainers = await _clientDashboardService.GetAllTrainersAsync();
        return Ok(trainers);
    }
}
