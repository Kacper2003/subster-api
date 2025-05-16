using Microsoft.AspNetCore.Mvc;
using Subster.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Subster.Models.UpdateModels;
using Subster.Models.InputModels;
using Subster.Models;
using Subster.Models.Dtos;
using Swashbuckle.AspNetCore.Annotations;

namespace Subster.API.Controllers;

[ApiController]
[Route("api/trainers")]
[Authorize(Roles = "Trainer")]
[Produces("application/json")]
[Consumes("application/json")]
[SwaggerTag("Þjálfarar")]
public class TrainersController(ITrainerService trainerService, IPaydayService paydayService) : ControllerBase
{
    private readonly ITrainerService _trainerService = trainerService;
    private readonly IPaydayService _paydayService = paydayService;

	[HttpGet("me")]
    [SwaggerOperation(
        Summary     = "Sækja prófíl þjálfara",
        Description = "Skilar upplýsingum um núverandi innskráðan þjálfara."
    )]
    [SwaggerResponse(200, "TrainerDetailsDto hlutur", typeof(TrainerDetailsDto))]
    public async Task<ActionResult> GetProfile()
    {
        var trainerSsn = User.Claims.FirstOrDefault(c => c.Type == "Ssn")?.Value;
        if (string.IsNullOrEmpty(trainerSsn))
            return Unauthorized("SSN not found in token");

		TrainerDetailsDto trainer = await _trainerService.GetTrainerDetailsBySsnAsync(trainerSsn);
        return Ok(trainer);
    }

    [HttpPut("me")]
    [SwaggerOperation(
        Summary     = "Uppfæra prófíl þjálfara",
        Description = "Uppfærir persónuupplýsingar fyrir innskráðan þjálfara."
    )]
    [SwaggerResponse(200, "TrainerDetailsDto hlutur", typeof(TrainerDetailsDto))]
    [SwaggerResponse(400, "Gildisvilla í innslagi", typeof(ApiError))]
    public async Task<IActionResult> UpdateProfile([FromBody] TrainerUpdateModel updateModel)
    {
        var trainerSsn = User.Claims.FirstOrDefault(c => c.Type == "Ssn")?.Value;
        if (string.IsNullOrEmpty(trainerSsn))
            return Unauthorized("SSN not found in token");

		TrainerDetailsDto updatedTrainer = await _trainerService.UpdateTrainerDetailsAsync(trainerSsn, updateModel);
        return NoContent();
    }

    [HttpGet("me/payday")]
    [SwaggerOperation(
        Summary     = "Staðfesta Payday-samþættingu",
        Description = "Skoðar hvort innskráði þjálfari hafi sett up samþættingu við Payday."
    )]
    [SwaggerResponse(200, "IntegrationDto hlutur", typeof(IntegrationDto))]
    public async Task<IActionResult> CheckPaydayCredentials()
    {
        var ssn = User.Claims.FirstOrDefault(c => c.Type == "Ssn")?.Value;
        if (string.IsNullOrEmpty(ssn))
            return Unauthorized("SSN not found in token");

        var credentials = await _trainerService.HasPaydayCredentialsAsync(ssn);
        return Ok(credentials);
    }

    [HttpPost("me/payday")]
    [SwaggerOperation(
        Summary     = "Uppfæra Payday-samþættingu",
        Description = "Setur inn eða uppfærir Payday-samþættingu fyrir þjálfara og staðfestir hana."
    )]
    [SwaggerResponse(204, "Aðgerð tókst")]
    [SwaggerResponse(401, "Payday upplýsingar rangar", typeof(ApiError))]
    public async Task<IActionResult> UpdatePaydayCredentials([FromBody] PaydayCredentialInputModel inputModel)
    {
        var ssn = User.Claims.FirstOrDefault(c => c.Type == "Ssn")?.Value;
        if (string.IsNullOrEmpty(ssn))
            return Unauthorized("SSN not found in token");

        await _paydayService.UpdateCredentials(ssn, inputModel.ClientId, inputModel.ClientSecret);
        return NoContent();
    }

    [HttpDelete("me/payday")]
    [SwaggerOperation(
        Summary     = "Eyða Payday-samþættingu",
        Description = "Fjarlægir Payday-samþættingu fyrir þjálfara."
    )]
    [SwaggerResponse(204, "Aðgerð tókst")]
    [SwaggerResponse(409, "Röng aðgerð (engin samþætting til staðar)", typeof(ApiError))]
    public async Task<IActionResult> DeletePaydayCredentials()
    {
        var ssn = User.Claims.FirstOrDefault(c => c.Type == "Ssn")?.Value;
        if (string.IsNullOrEmpty(ssn))
            return Unauthorized("SSN not found in token");

        await _paydayService.DeleteCredentials(ssn);
        return NoContent();
    }
}
