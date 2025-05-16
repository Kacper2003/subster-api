using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Subster.API.Services.Interfaces;
using Subster.API.Services;
using Subster.Models.InputModels;
using Subster.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace Subster.API.Controllers;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
[Consumes("application/json")]
[SwaggerTag("Auðkenning")]
public class AuthController(
	ITaktikalAuthService taktikalAuthService,
	JwtService jwtService,
	ITrainerService trainerService,
	IClientService clientService) : ControllerBase
{
    private readonly ITaktikalAuthService _taktikalAuthService = taktikalAuthService;
    private readonly JwtService _jwtService = jwtService;
    private readonly ITrainerService _trainerService = trainerService;
    private readonly IClientService _clientService = clientService;

	[HttpPost("login/trainer")]
    [SwaggerOperation(
        Summary     = "Innskrá þjálfara",
        Description = "Auðkennir þjálfara með rafrænum skilríkjum og setur JWT-köku í vafra."
    )]
    [SwaggerResponse(200, "Innskráning tókst")]
    [SwaggerResponse(401, "Innskráning mistókst (rangar upplýsingar)", typeof(AuthResult))]
    public async Task<IActionResult> LoginTrainer([FromBody] AuthInputModel inputModel)
    {
		Models.ResponseModels.EndAuthResponseModel authResult = await _taktikalAuthService.AuthenticateAsync(inputModel);
        if (!authResult.Authenticated)
        {
            return Unauthorized(authResult);
        }

        await _trainerService.CreateTrainerIfNotExistsAsync(new UserInputModel
        {
            Ssn = authResult.Customer.Ssn,
            Name = authResult.Customer.Name
        });

        var token = _jwtService.GenerateToken(
            authResult.Customer.Ssn,
            authResult.Customer.Name,
            "Trainer");

        Response.Cookies.Append("jwt", token, new CookieOptions { HttpOnly = true });

        return Ok(new { Authenticated = true });
    }

    [HttpPost("login/client")]
    [SwaggerOperation(
        Summary     = "Innskrá viðskiptavin",
        Description = "Auðkennir viðskiptavin með rafrænum skilríkjum og setur JWT-köku í vafra."   
    )]
    [SwaggerResponse(200, "Innskráning tókst")]
    [SwaggerResponse(401, "Innskráning mistókst (rangar upplýsingar)", typeof(AuthResult))]
    public async Task<IActionResult> LoginClient([FromBody] AuthInputModel inputModel)
    {
		Models.ResponseModels.EndAuthResponseModel authResult = await _taktikalAuthService.AuthenticateAsync(inputModel);
        if (!authResult.Authenticated)
        {
            return Unauthorized(authResult);
        }

        await _clientService.CreateClientIfNotExistsAsync(new UserInputModel
        {
            Ssn = authResult.Customer.Ssn,
            Name = authResult.Customer.Name
        });

        var token = _jwtService.GenerateToken(
            authResult.Customer.Ssn,
            authResult.Customer.Name,
            "Client");

        Response.Cookies.Append("jwt", token, new CookieOptions { HttpOnly = true });

        return Ok(new { Authenticated = true });
    }

    [HttpPost("logout")]
    [SwaggerOperation(
        Summary     = "Útskrá notanda",
        Description = "Eyðir JWT-köku og skráir notanda út."
    )]
    [SwaggerResponse(200, "Útskráning tókst")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("jwt");
        return Ok(new { Authenticated = false });
    }

    [Authorize]
    [HttpGet("me")]
    [SwaggerOperation(
        Summary     = "Sækja notendaskilríki",
        Description = "Skilar öllum gögnum úr JWT fyrir innskráðan notanda (notað sem athugun á hvort notandi sé auðkenndur)."
    )]
    [SwaggerResponse(200, "Upplýsingar úr JWT", typeof(IDictionary<string, string>))]
    [SwaggerResponse(401, "Óheimilt – innskráning ekki gild")]
    public IActionResult GetUserClaims()
    {
        var claims = User.Claims.ToDictionary(c => c.Type, c => c.Value);
        return Ok(claims);
    }
}
