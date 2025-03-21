using Microsoft.EntityFrameworkCore;
using Subster.DAL.Interfaces;
using Subster.DAL.Entities;
using Subster.Models.Dtos;
using Subster.Models.InputModels;

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

    public async Task<UserDto?> GetUserBySsnAsync(string ssn)
    {
        return await _dbContext.Users
            .Where(u => u.Ssn == ssn)
            .Select(u => new UserDto
            {
                Id = u.Id,
                Name = u.Name,
                Ssn = u.Ssn,
                PhoneNumber = u.PhoneNumber
            })
            .FirstOrDefaultAsync();
    }

    public async Task<User?> GetUserEntityBySsnAsync(string ssn)
    {
        return await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Ssn == ssn);
    }

    public async Task CreateUserAsync(UserInputModel inputModel)
    {
        // Check if user already exists
        var existingUser = _dbContext.Users
            .FirstOrDefault(u => u.Ssn == inputModel.Ssn);

        if (existingUser == null)
        {
            var user = new User
            {
                Name = inputModel.Name,
                Ssn = inputModel.Ssn,
                PhoneNumber = "123-4567",
                CreatedAt = DateTime.UtcNow
            };

            // Save new user to database
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();
        } else {
            // Update user name if it has changed
            existingUser.Name = inputModel.Name;
            await _dbContext.SaveChangesAsync();
        }
    }
    
    public async Task UpdatePaydayCredentialsAsync(int userId, string? clientId, string? clientSecret)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user != null)
        {
            user.PaydayClientId = clientId;
            user.PaydayClientSecret = clientSecret;
            await _dbContext.SaveChangesAsync();
        }
    }
}
