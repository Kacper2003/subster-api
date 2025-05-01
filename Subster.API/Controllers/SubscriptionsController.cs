using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Subster.API.Services.Interfaces;
using Subster.Models.InputModels;

namespace Subster.API.Controllers;

[ApiController]
[Route("api/[controller]")]
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

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateSubscription([FromBody] SubscriptionInputModel inputModel)
    {
        // Get the user's SSN from the token
        var trainerSsn = User.Claims.FirstOrDefault(c => c.Type == "Ssn")?.Value;
        if (trainerSsn == null)
        {
            return BadRequest("SSN not found in token");
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

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetSubscriptions()
    {
        // Get the user's SSN from the token
        var trainerSsn = User.Claims.FirstOrDefault(c => c.Type == "Ssn")?.Value;
        if (trainerSsn == null)
        {
            return BadRequest("SSN not found in token");
        }

        var subscriptions = await _subscriptionService.GetSubscriptionsAsync(trainerSsn);
        return Ok(subscriptions);
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetSubscriptionById(int id)
    {
        // Get the user's SSN from the token
        var trainerSsn = User.Claims.FirstOrDefault(c => c.Type == "Ssn")?.Value;
        if (trainerSsn == null)
        {
            return BadRequest("SSN not found in token");
        }

        var subscription = await _subscriptionService.GetSubscriptionByIdAsync(trainerSsn, id);
        if (subscription == null)
        {
            return NotFound();
        }

        return Ok(subscription);
    }

    // [Authorize]
    // [HttpGet("{id}/invoices")]
    // public async Task<IActionResult> GetInvoicesBySubscriptionId(int id)
    // {
    //     // Get the user's SSN from the token
    //     var trainerSsn = User.Claims.FirstOrDefault(c => c.Type == "Ssn")?.Value;
    //     if (trainerSsn == null)
    //     {
    //         return BadRequest("SSN not found in token");
    //     }

    //     var invoices = await _subscriptionService.GetInvoicesBySubscriptionIdAsync(trainerSsn, id);
    //     if (invoices == null)
    //     {
    //         return NotFound();
    //     }

    //     return Ok(invoices);
    // }
}