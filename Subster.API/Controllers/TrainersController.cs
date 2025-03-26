using Microsoft.AspNetCore.Mvc;
using Subster.DAL.Entities;

using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;
using Subster.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace Subster.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TrainersController : ControllerBase
{
    private readonly ITrainerService _trainerService;

    public TrainersController(ITrainerService trainerService)
    {
        _trainerService = trainerService;
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Trainer>>> GetAllTrainers()
    {
        var trainers = await _trainerService.GetAllTrainersAsync();
        return Ok(trainers);
    }
}