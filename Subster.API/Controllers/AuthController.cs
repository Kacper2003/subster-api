using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Subster.API.Services;
using Subster.API.Services.Interfaces;
using Subster.Models.InputModels;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;


namespace Subster.API.Controllers;
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ITaktikalAuthService _taktikalAuthService;
    private readonly JwtService _jwtService;
    private readonly IUserService _userService;

    public AuthController(ITaktikalAuthService taktikalAuthService, JwtService jwtService, IUserService userService)
    {
        _taktikalAuthService = taktikalAuthService;
        _jwtService = jwtService;
        _userService = userService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] AuthInputModel inputModel)
    {
        var authResult = await _taktikalAuthService.AuthenticateAsync(inputModel);
        if (!authResult.Authenticated)
        {
            return BadRequest(authResult);
        }

        await _userService.CreateUserIfNotExistsAsync(new UserInputModel
        {
            Ssn = authResult.Customer.Ssn,
            Name = authResult.Customer.Name
        });

        var token = _jwtService.GenerateToken(authResult.Customer.Ssn, authResult.Customer.Name);

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

        foreach (var claim in claims)
        {
            Console.WriteLine($"{claim.Key}: {claim.Value}");
        }

        return Ok(claims);
    }
}
