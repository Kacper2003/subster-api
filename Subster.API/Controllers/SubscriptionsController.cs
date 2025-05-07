using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Subster.API.Services.Interfaces;
using Subster.Models;
using Subster.Models.InputModels;
using Subster.Models.UpdateModels;
using Subster.Models.Dtos;

namespace Subster.API.Controllers;

[ApiController]
[Route("api/subscriptions")]
[Authorize(Roles = "Trainer")]
[Produces("application/json")]
[Consumes("application/json")]
public class SubscriptionsController : ControllerBase
{
    private readonly ITaktikalAuthService _taktikalAuthService;
    private readonly ISubscriptionService _subscriptionService;
    private readonly IClientService _clientService;
    private readonly IProgramService _programService;

    public SubscriptionsController(ITaktikalAuthService taktikalAuthService, ISubscriptionService subscriptionService, IClientService clientService, IProgramService programService)
    {
        _taktikalAuthService = taktikalAuthService;
        _subscriptionService = subscriptionService;
        _clientService = clientService;
        _programService = programService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<SubscriptionDto>), 200)]
    public async Task<IActionResult> GetSubscriptions()
    {
        var trainerSsn = User.Claims.FirstOrDefault(c => c.Type == "Ssn")?.Value;
        if (string.IsNullOrEmpty(trainerSsn))
        {
            return Unauthorized("SSN not found in token");
        }

        var subscriptions = await _subscriptionService.GetAllSubscriptionsAsync(trainerSsn);
        return Ok(subscriptions);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SubscriptionDetailsDto), 200)]
    public async Task<IActionResult> GetSubscriptionById(Guid id)
    {
        var trainerSsn = User.Claims.FirstOrDefault(c => c.Type == "Ssn")?.Value;
        if (string.IsNullOrEmpty(trainerSsn))
        {
            return Unauthorized("SSN not found in token");
        }

        var subscription = await _subscriptionService.GetSubscriptionByIdAsync(trainerSsn, id);

        return Ok(subscription);
    }

    [HttpPost]
    [ProducesResponseType(typeof(SubscriptionDetailsDto), 201)]
    [ProducesResponseType(typeof(ApiError), 400)]
    public async Task<IActionResult> CreateSubscription([FromBody] SubscriptionInputModel inputModel)
    {
        var trainerSsn = User.Claims.FirstOrDefault(c => c.Type == "Ssn")?.Value;
        if (string.IsNullOrEmpty(trainerSsn))
        {
            return Unauthorized("SSN not found in token");
        }

        var program = await _programService.GetProgramByIdAsync(trainerSsn, inputModel.ProgramId);
        if (program == null)
        {
            return NotFound("Program not found");
        }
        
        // Get confirmation (authentication) from client
        var clientAuthResult = await _taktikalAuthService.AuthenticateAsync(new AuthInputModel
        {
            PhoneNumber = inputModel.ClientPhoneNumber,
            Ssn = inputModel.ClientSsn
        });

        if (!clientAuthResult.Authenticated)
        {
            return BadRequest(clientAuthResult);
        }

        // Create client if it doesn't exist
        var clientId = await _clientService.CreateClientIfNotExistsAsync(new UserInputModel
        {
            Name = clientAuthResult.Customer.Name,
            Ssn = clientAuthResult.Customer.Ssn
        });

        // Finally, create the subscription
        await _subscriptionService.CreateSubscriptionAsync(inputModel, trainerSsn, clientId, clientAuthResult.Customer.Ssn);

        return Created();
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(SubscriptionDetailsDto), 200)]
    [ProducesResponseType(typeof(ApiError), 400)]
    public async Task<IActionResult> UpdateSubscription(Guid id, [FromBody] SubscriptionUpdateModel updateModel)
    {
        var trainerSsn = User.Claims.FirstOrDefault(c => c.Type == "Ssn")?.Value;
        if (string.IsNullOrEmpty(trainerSsn))
        {
            return Unauthorized("SSN not found in token");
        }

        var updatedSubscription = await _subscriptionService.UpdateSubscriptionAsync(trainerSsn, id, updateModel);
        return Ok(updatedSubscription);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiError), 409)]
    public async Task<IActionResult> DeactivateSubscription(Guid id)
    {
        var trainerSsn = User.Claims.FirstOrDefault(c => c.Type == "Ssn")?.Value;
        if (string.IsNullOrEmpty(trainerSsn))
        {
            return Unauthorized("SSN not found in token");
        }

        await _subscriptionService.DeactivateSubscriptionAsync(trainerSsn, id);
        return NoContent();
    }
}