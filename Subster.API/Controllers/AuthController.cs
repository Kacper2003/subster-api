using Microsoft.AspNetCore.Mvc;
using Subster.API.Services;
using Subster.API.Services.Interfaces;
using Subster.Models.InputModels;


namespace Subster.API.Controllers;
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ITaktikalAuthService _taktikalAuthService;
    private readonly JwtService _jwtService;

    public AuthController(ITaktikalAuthService taktikalAuthService, JwtService jwtService)
    {
        _taktikalAuthService = taktikalAuthService;
        _jwtService = jwtService;
    }

    [HttpPost]
    public async Task<IActionResult> AuthenticateUser([FromBody] AuthInputModel inputModel)
    {
        var authResult = await _taktikalAuthService.AuthenticateAsync(inputModel);
        if (!authResult.Authenticated)
        {
            return BadRequest(authResult);
        }

        var token = _jwtService.GenerateToken(authResult.Customer.Ssn, authResult.Customer.Name);

        return Ok(new {
            Authenticated = true,
            Token = token,
            Customer = authResult.Customer
        });
    }
}

