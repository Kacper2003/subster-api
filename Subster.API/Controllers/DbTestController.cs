using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Subster.DAL;


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
    public async Task<IActionResult> TestDatabaseConnection()
    {
        try
        {
            var canConnect = await _context.Database.CanConnectAsync();
            return Ok(new { Success = canConnect });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = ex.Message });
        }
    }
}