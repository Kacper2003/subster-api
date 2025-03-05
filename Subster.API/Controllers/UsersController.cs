using Microsoft.AspNetCore.Mvc;
using Subster.DAL.Entities;

using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;
using Subster.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;


[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<User>>> GetAllUsers()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
    }
}