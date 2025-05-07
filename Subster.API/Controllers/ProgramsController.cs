using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Subster.API.Services.Interfaces;
using Subster.Models.InputModels;
using Subster.Models.UpdateModels;
using Subster.Models.Dtos;
using Subster.Models;

namespace Subster.API.Controllers;

[ApiController]
[Authorize(Roles = "Trainer")]
[Route("api/programs")]
[Produces("application/json")]
[Consumes("application/json")]
public class ProgramsController : ControllerBase
{
    private readonly IProgramService _programService;

    public ProgramsController(IProgramService programService)
    {
        _programService = programService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProgramDto>), 200)]
    public async Task<IActionResult> GetPrograms()
    {
        var trainerSsn = User.Claims.FirstOrDefault(c => c.Type == "Ssn")?.Value;
        if (string.IsNullOrEmpty(trainerSsn))
        {
            return Unauthorized("SSN not found in token");
        } 

        var programs = await _programService.GetAllProgramsAsync(trainerSsn);
        return Ok(programs);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProgramDto), 200)]
    public async Task<IActionResult> GetProgramById(Guid id)
    {
        var trainerSsn = User.Claims.FirstOrDefault(c => c.Type == "Ssn")?.Value;
        if (string.IsNullOrEmpty(trainerSsn))
        {
            return Unauthorized("SSN not found in token");
        }

        var program = await _programService.GetProgramByIdAsync(trainerSsn, id);

        return Ok(program);
    }


    [HttpPost]
    [ProducesResponseType(typeof(ProgramDto), 201)]
    [ProducesResponseType(typeof(ApiError), 400)]
    public async Task<IActionResult> CreateProgram([FromBody] ProgramInputModel inputModel)
    {
        var trainerSsn = User.Claims.FirstOrDefault(c => c.Type == "Ssn")?.Value;
        if (string.IsNullOrEmpty(trainerSsn))
        {
            return Unauthorized("SSN not found in token");
        }
        var program = await _programService.CreateProgramAsync(inputModel, trainerSsn);
        return CreatedAtAction(nameof(GetProgramById), new { id = program.Id }, program);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ProgramDto), 200)]
    [ProducesResponseType(typeof(ApiError), 400)]
    public async Task<IActionResult> UpdateProgram(Guid id, [FromBody] ProgramUpdateModel updateModel)
    {
        var trainerSsn = User.Claims.FirstOrDefault(c => c.Type == "Ssn")?.Value;
        if (string.IsNullOrEmpty(trainerSsn))
        {
            return Unauthorized("SSN not found in token");
        }

        var updatedProgram = await _programService.UpdateProgramAsync(id, updateModel, trainerSsn);
        return Ok(updatedProgram);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiError), 409)]
    public async Task<IActionResult> DeactivateProgram(Guid id)
    {
        var trainerSsn = User.Claims.FirstOrDefault(c => c.Type == "Ssn")?.Value;
        if (string.IsNullOrEmpty(trainerSsn))
        {
            return Unauthorized("SSN not found in token");
        }

        await _programService.DeactivateProgramAsync(trainerSsn, id);
        return NoContent();
    }
}