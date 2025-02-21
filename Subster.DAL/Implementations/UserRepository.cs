using Microsoft.EntityFrameworkCore;
using Subster.DAL.Interfaces;
using Subster.Models.Dtos;

namespace Subster.DAL.Implementations;

public class UserRepository : IUserRepository
{
    private readonly SubsterDbContext _dbContext;

    public UserRepository(SubsterDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        return await _dbContext.Users
            .Select(u => new UserDto
            {
                Id = u.Id,
                Name = u.Name,
                Ssn = u.Ssn,
                PhoneNumber = u.PhoneNumber
            })
            .ToListAsync();
    }
}
