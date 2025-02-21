using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Subster.DAL;
using Subster.DAL.Entities;

using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;
using Subster.API.Services.Interfaces;


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
    public async Task<ActionResult<IEnumerable<User>>> GetAllUsers()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
    }
}