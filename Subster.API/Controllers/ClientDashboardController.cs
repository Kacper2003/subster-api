using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Subster.API.Services.Interfaces;
using Subster.Models.Dtos;
using Subster.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace Subster.API.Controllers;

[ApiController]
[Route("api/client")]
[Produces("application/json")]
[Consumes("application/json")]
[SwaggerTag("Viðmót viðskiptavinar")]
public class ClientDashboardController(IClientDashboardService clientDashboardService) : ControllerBase
{
    private readonly IClientDashboardService _clientDashboardService = clientDashboardService;

	[HttpGet("subscriptions")]
    [Authorize(Roles = "Client")]
    [SwaggerOperation(
        Summary     = "Sækja áskriftir",
        Description = "Skilar öllum áskriftum sem tengjast innskráðum viðskiptavini."
    )]
    [SwaggerResponse(200, "Listi af SubscriptionDto hlutum", typeof(IEnumerable<SubscriptionDto>))]
    [SwaggerResponse(401, "Óheimilt – innskráning ekki gild", typeof(ApiError))]
    public async Task<IActionResult> GetSubscriptions()
    {
        var clientSsn = User.Claims.FirstOrDefault(c => c.Type == "Ssn")?.Value;
        if (string.IsNullOrEmpty(clientSsn))
        {
            return Unauthorized("SSN not found in token");
        }

		IEnumerable<SubscriptionDto> subscriptions = await _clientDashboardService.GetSubscriptionsByClientSsnAsync(clientSsn);
        return Ok(subscriptions);
    }
    
    [HttpDelete("subscriptions/{id:guid}")]
    [Authorize(Roles = "Client")]
    [SwaggerOperation(
        Summary     = "Óvirkja áskrift",
        Description = "Merkir áskrift sem óvirka. Reikningar hætta að sendast en viðskiptavinur hefur aðgang samkvæmt síðasta greidda reikning."
    )]
    [SwaggerResponse(204, "Aðgerð tókst")]
    [SwaggerResponse(409, "Röng aðgerð (áskrift nú þegar óvirk)", typeof(ApiError))]
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
    [SwaggerOperation(
        Summary     = "Sækja alla þjálfara",
        Description = "Skilar lista af öllum þjálfurum sem viðskiptavinur getur valið."
    )]
    [SwaggerResponse(200, "Listi af TrainerDto hlutum", typeof(IEnumerable<TrainerDto>))]
    public async Task<IActionResult> GetTrainers()
    {
		IEnumerable<TrainerDto> trainers = await _clientDashboardService.GetAllTrainersAsync();
        return Ok(trainers);
    }
}
