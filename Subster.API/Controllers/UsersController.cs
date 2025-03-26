using Microsoft.AspNetCore.Mvc;
using Subster.DAL.Entities;

using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;
using Subster.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace Subster.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetAllUsers()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
    }
}