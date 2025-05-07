using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Subster.API.Services;
using Subster.API.Services.Interfaces;
using Subster.Models.InputModels;

namespace Subster.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Consumes("application/json")]
public class AuthController : ControllerBase
{
    private readonly ITaktikalAuthService _taktikalAuthService;
    private readonly JwtService _jwtService;
    private readonly ITrainerService _trainerService;
    private readonly IClientService _clientService;

    public AuthController(ITaktikalAuthService taktikalAuthService, JwtService jwtService, ITrainerService trainerService, IClientService clientService)
    {
        _taktikalAuthService = taktikalAuthService;
        _jwtService = jwtService;
        _trainerService = trainerService;
        _clientService = clientService;
    }

    [HttpPost("login/trainer")]
    public async Task<IActionResult> LoginTrainer([FromBody] AuthInputModel inputModel)
    {
        var authResult = await _taktikalAuthService.AuthenticateAsync(inputModel);
        if (!authResult.Authenticated)
        {
            return BadRequest(authResult);
        }

        var firstTimeLogin = await _trainerService.CreateTrainerIfNotExistsAsync(new UserInputModel
        {
            Ssn = authResult.Customer.Ssn,
            Name = authResult.Customer.Name
        });

        var token = _jwtService.GenerateToken(authResult.Customer.Ssn, authResult.Customer.Name, "Trainer");

        Response.Cookies.Append("jwt", token, new CookieOptions
        {
            HttpOnly = true
        });

        return Ok(new {
            Authenticated = true,
            Customer = authResult.Customer,
            IsFirstTimeLogin = firstTimeLogin
        });
    }

    [HttpPost("login/client")]
    public async Task<IActionResult> LoginClient([FromBody] AuthInputModel inputModel)
    {
        var authResult = await _taktikalAuthService.AuthenticateAsync(inputModel);
        if (!authResult.Authenticated)
        {
            return BadRequest(authResult);
        }

        await _clientService.CreateClientIfNotExistsAsync(new UserInputModel
        {
            Ssn = authResult.Customer.Ssn,
            Name = authResult.Customer.Name
        });

        var token = _jwtService.GenerateToken(authResult.Customer.Ssn, authResult.Customer.Name, "Client");

        Response.Cookies.Append("jwt", token, new CookieOptions
        {
            HttpOnly = true
        });

        return Ok(new {
            Authenticated = true,
            Customer = authResult.Customer
        });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("jwt");
        return Ok(new {
            Authenticated = false
        });
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult GetUserClaims()
    {
        var claims = User.Claims.ToDictionary(c => c.Type, c => c.Value);

        // foreach (var claim in claims)
        // {
        //     Console.WriteLine($"{claim.Key}: {claim.Value}");
        // }

        return Ok(claims);
    }
}
