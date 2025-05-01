using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Subster.API.Services;
using Subster.API.Services.Interfaces;
using Subster.Models.Dtos.Payday;
using Subster.Models.InputModels;

namespace Subster.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaydayController : ControllerBase
{
    private readonly IPaydayService _paydayService;

    public PaydayController(IPaydayService paydayService)
    {
        _paydayService = paydayService;
    }

    [Authorize(Roles = "Trainer")]
    [HttpPost("credentials")]
    public async Task<IActionResult> UpdateCredentials([FromBody] PaydayCredentialInputModel inputModel)
    {
        var ssn = User.Claims.FirstOrDefault(c => c.Type == "Ssn")?.Value;
        if (string.IsNullOrEmpty(ssn))
        {
            return Unauthorized("SSN not found in token");
        }

        bool updated = await _paydayService.UpdateCredentials(ssn, inputModel.ClientId, inputModel.ClientSecret);
        if (!updated)
        {
            return BadRequest(new { Error = "Client ID or Client Secret is invalid" });
        }
        return Ok();
    }

    [Authorize(Roles = "Trainer")]
    [HttpDelete("credentials")]
    public async Task<IActionResult> DeleteCredentials()
    {
        var ssn = User.Claims.FirstOrDefault(c => c.Type == "Ssn")?.Value;
        if (string.IsNullOrEmpty(ssn))
        {
            return Unauthorized("SSN not found in token");
        }

        bool deleted = await _paydayService.DeleteCredentials(ssn);
        if (!deleted)
        {
            return BadRequest(new { Error = "Could not delete credentials" });
        }
        return Ok();
    }
}
