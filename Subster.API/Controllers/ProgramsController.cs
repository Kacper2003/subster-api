using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Subster.API.Services.Interfaces;
using Subster.Models.InputModels;

namespace Subster.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProgramsController : ControllerBase
{
    private readonly IProgramService _programService;

    public ProgramsController(IProgramService programService)
    {
        _programService = programService;
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateProgram([FromBody] ProgramInputModel inputModel)
    {
        var ssn = User.Claims.FirstOrDefault(c => c.Type == "Ssn")?.Value;
        if (string.IsNullOrEmpty(ssn))
        {
            return Unauthorized("SSN not found in token");
        }
        await _programService.CreateProgramAsync(inputModel, ssn);
        return Created();
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetPrograms()
    {
        var ssn = User.Claims.FirstOrDefault(c => c.Type == "Ssn")?.Value;
        if (string.IsNullOrEmpty(ssn))
        {
            return Unauthorized("SSN not found in token");
        }

        var programs = await _programService.GetAllProgramsAsync(ssn);
        return Ok(programs);
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProgramById(int id)
    {
        var ssn = User.Claims.FirstOrDefault(c => c.Type == "Ssn")?.Value;
        if (string.IsNullOrEmpty(ssn))
        {
            return Unauthorized("SSN not found in token");
        }

        var program = await _programService.GetProgramByIdAsync(ssn, id);
        if (program == null)
        {
            return NotFound();
        }

        return Ok(program);
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeactivateProgram(int id)
    {
        var ssn = User.Claims.FirstOrDefault(c => c.Type == "Ssn")?.Value;
        if (string.IsNullOrEmpty(ssn))
        {
            return Unauthorized("SSN not found in token");
        }

        await _programService.DeactivateProgramAsync(ssn, id);
        return NoContent();
    }
}