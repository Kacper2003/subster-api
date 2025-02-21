using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Subster.DAL;
using Subster.DAL.Entities;


[Microsoft.AspNetCore.Mvc.Route("api/test-db")]
[ApiController]
public class DbTestController : ControllerBase
{
    private readonly SubsterDbContext _context;

    public DbTestController(SubsterDbContext context)
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