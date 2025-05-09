using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Subster.API.Services.Interfaces;
using Subster.Models;
using Subster.Models.InputModels;
using Subster.Models.UpdateModels;
using Subster.Models.Dtos;
using Swashbuckle.AspNetCore.Annotations;

namespace Subster.API.Controllers;

[ApiController]
[Route("api/subscriptions")]
[Authorize(Roles = "Trainer")]
[Produces("application/json")]
[Consumes("application/json")]
[SwaggerTag("Áskriftir")]
public class SubscriptionsController : ControllerBase
{
    private readonly ITaktikalAuthService _taktikalAuthService;
    private readonly ISubscriptionService _subscriptionService;
    private readonly IClientService _clientService;
    private readonly IProgramService _programService;

    public SubscriptionsController(
        ITaktikalAuthService taktikalAuthService,
        ISubscriptionService subscriptionService,
        IClientService clientService,
        IProgramService programService)
    {
        _taktikalAuthService = taktikalAuthService;
        _subscriptionService = subscriptionService;
        _clientService = clientService;
        _programService = programService;
    }

    [HttpGet]
    [SwaggerOperation(
        Summary     = "Sækja allar áskriftir",
        Description = "Skilar lista af öllum áskriftum sem tengjast innskráðum þjálfara."
    )]
    [SwaggerResponse(200, "Listi af SubscriptionDto hlutum", typeof(IEnumerable<SubscriptionDto>))]
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
    [SwaggerOperation(
        Summary     = "Sækja áskrift eftir auðkenni",
        Description = "Skilar upplýsingum um eina áskrift með gefnu ID."
    )]
    [SwaggerResponse(200, "SubscriptionDetailsDto hlutur", typeof(SubscriptionDetailsDto))]
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
    [SwaggerOperation(
        Summary     = "Búa til nýja áskrift",
        Description = "Býr til nýja áskrift fyrir innskráðan þjálfara."
    )]
    [SwaggerResponse(201, "Nýr SubscriptionDetailsDto hlutur", typeof(SubscriptionDetailsDto))]
    [SwaggerResponse(400, "Gildisvilla í innslagi", typeof(ApiError))]
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

        var clientAuthResult = await _taktikalAuthService.AuthenticateAsync(new AuthInputModel
        {
            PhoneNumber = inputModel.ClientPhoneNumber,
            Ssn         = inputModel.ClientSsn
        });

        if (!clientAuthResult.Authenticated)
        {
            return BadRequest(clientAuthResult);
        }

        var clientId = await _clientService.CreateClientIfNotExistsAsync(new UserInputModel
        {
            Name = clientAuthResult.Customer.Name,
            Ssn  = clientAuthResult.Customer.Ssn
        });

        await _subscriptionService.CreateSubscriptionAsync(
            inputModel, trainerSsn, clientId, clientAuthResult.Customer.Ssn);

        return Created();
    }

    [HttpPut("{id:guid}")]
    [SwaggerOperation(
        Summary     = "Uppfæra áskrift",
        Description = "Uppfærir tiltekna áskrift með nýjum gögnum."
    )]
    [SwaggerResponse(200, "Uppfærður SubscriptionDetailsDto hlutur", typeof(SubscriptionDetailsDto))]
    [SwaggerResponse(400, "Gildisvilla í innslagi", typeof(ApiError))]
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
    [SwaggerOperation(
        Summary     = "Óvirkja áskrift",
        Description = "Merkir áskrift sem óvirka svo hún birtist ekki lengur. Reikningar hætta að sendast en viðskiptavinur hefur aðgang samkvæmt síðasta greidda reikning."
    )]
    [SwaggerResponse(204, "Aðgerð tókst")]
    [SwaggerResponse(409, "Röng aðgerð (áskrift nú þegar óvirk)", typeof(ApiError))]
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
