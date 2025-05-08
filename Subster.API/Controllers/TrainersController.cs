using Microsoft.AspNetCore.Mvc;
using Subster.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Subster.Models.UpdateModels;
using Subster.Models.InputModels;
using Subster.Models;
using Subster.Models.Dtos;

namespace Subster.API.Controllers;

[ApiController]
[Route("api/trainers")]
[Authorize(Roles = "Trainer")]
[Produces("application/json")]
[Consumes("application/json")]
public class TrainersController : ControllerBase
{
    private readonly ITrainerService _trainerService;
    private readonly IPaydayService _paydayService;


    public TrainersController(ITrainerService trainerService, IPaydayService paydayService)
    {
        _trainerService = trainerService;
        _paydayService = paydayService;
    }

    [HttpGet("me")]
    [ProducesResponseType(typeof(TrainerDetailsDto), 200)]
    public async Task<ActionResult> GetProfile()
    {
        var trainerSsn = User.Claims.FirstOrDefault(c => c.Type == "Ssn")?.Value;
        if (string.IsNullOrEmpty(trainerSsn))
        {
            return Unauthorized("SSN not found in token");
        }

        var trainer = await _trainerService.GetTrainerDetailsBySsnAsync(trainerSsn);
        return Ok(trainer);
    }

    [HttpPut("me")]
    [ProducesResponseType(typeof(TrainerDetailsDto), 200)]
    [ProducesResponseType(typeof(ApiError), 400)]
    public async Task<IActionResult> UpdateProfile([FromBody] TrainerUpdateModel updateModel)
    {
        var trainerSsn = User.Claims.FirstOrDefault(c => c.Type == "Ssn")?.Value;
        if (string.IsNullOrEmpty(trainerSsn))
        {
            return Unauthorized("SSN not found in token");
        }

        var updatedTrainer = await _trainerService.UpdateTrainerDetailsAsync(trainerSsn, updateModel);
        return NoContent();
    }

    [HttpGet("me/payday")]
    [ProducesResponseType(typeof(IntegrationDto), 200)]
    public async Task<IActionResult> CheckPaydayCredentials()
    {
        var ssn = User.Claims.FirstOrDefault(c => c.Type == "Ssn")?.Value;
        if (string.IsNullOrEmpty(ssn))
        {
            return Unauthorized("SSN not found in token");
        }

        var credentials = await _trainerService.HasPaydayCredentialsAsync(ssn);
        return Ok(credentials);
    }


    [HttpPost("me/payday")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiError), 401)]
    public async Task<IActionResult> UpdatePaydayCredentials([FromBody] PaydayCredentialInputModel inputModel)
    {
        var ssn = User.Claims.FirstOrDefault(c => c.Type == "Ssn")?.Value;
        if (string.IsNullOrEmpty(ssn))
        {
            return Unauthorized("SSN not found in token");
        }

        await _paydayService.UpdateCredentials(ssn, inputModel.ClientId, inputModel.ClientSecret);

        return NoContent();
    }

    [HttpDelete("me/payday")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiError), 409)]
    public async Task<IActionResult> DeletePaydayCredentials()
    {
        var ssn = User.Claims.FirstOrDefault(c => c.Type == "Ssn")?.Value;
        if (string.IsNullOrEmpty(ssn))
        {
            return Unauthorized("SSN not found in token");
        }

        await _paydayService.DeleteCredentials(ssn);
        return Ok();
    }

}