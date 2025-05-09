using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Subster.API.Services.Interfaces;
using Subster.Models.InputModels;
using Subster.Models.UpdateModels;
using Subster.Models.Dtos;
using Subster.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace Subster.API.Controllers;

[ApiController]
[Authorize(Roles = "Trainer")]
[Route("api/programs")]
[Produces("application/json")]
[Consumes("application/json")]
[SwaggerTag("Æfingaáætlanir")]
public class ProgramsController : ControllerBase
{
    private readonly IProgramService _programService;

    public ProgramsController(IProgramService programService)
    {
        _programService = programService;
    }

    [HttpGet]
    [SwaggerOperation(
        Summary     = "Sækja allar æfingaáætlanir",
        Description = "Skilar lista af öllum æfingaáætlunum sem tengjast innskráðum þjálfara."
    )]
    [SwaggerResponse(200, "Listi af ProgramDto hlutum", typeof(IEnumerable<ProgramDto>))]
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
    [SwaggerOperation(
        Summary     = "Sækja æfingaáætlun eftir auðkenni",
        Description = "Skilar æfingaáætlun með gefnu ID."
    )]
    [SwaggerResponse(200, "ProgramDto hlutur", typeof(ProgramDto))]
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
    [SwaggerOperation(
        Summary     = "Búa til nýja æfingaáætlun",
        Description = "Býr til nýja æfingaáætlun fyrir innskráðan þjálfara."
    )]
    [SwaggerResponse(201, "Nýr ProgramDto hlutur", typeof(ProgramDto))]
    [SwaggerResponse(400, "Gildisvilla í innslagi", typeof(ApiError))]
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
    [SwaggerOperation(
        Summary     = "Uppfæra æfingaáætlun",
        Description = "Uppfærir tiltekna æfingaáætlun með nýjum gögnum."
    )]
    [SwaggerResponse(200, "Uppfærður ProgramDto hlutur", typeof(ProgramDto))]
    [SwaggerResponse(400, "Gildisvilla í innslagi", typeof(ApiError))]
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
    [SwaggerOperation(
        Summary     = "Óvirkja æfingaáætlun",
        Description = "Merkir æfingaáætlun sem óvirka svo hún birtist ekki lengur."
    )]
    [SwaggerResponse(204, "Aðgerð tókst")]
    [SwaggerResponse(409, "Röng aðgerð (æfingaáætlun nú þegar óvirk)", typeof(ApiError))]
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
