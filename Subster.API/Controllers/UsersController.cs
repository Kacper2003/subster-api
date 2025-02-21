using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Subster.DAL;
using Subster.DAL.Entities;

using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;


[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly SubsterDbContext _context;

    public UsersController(SubsterDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetAllUsers()
    {
        var users = await _context.Users.ToListAsync();
        return Ok(users);
    }
}