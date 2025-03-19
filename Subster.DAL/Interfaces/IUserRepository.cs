using Subster.Models.Dtos;
using Subster.Models.InputModels;
using Subster.DAL.Entities;

namespace Subster.DAL.Interfaces;

public interface IUserRepository
{
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
    Task<UserDto?> GetUserBySsnAsync(string ssn);
    Task<User?> GetUserEntityBySsnAsync(string ssn);
    Task CreateUserAsync(UserInputModel inputModel);
    Task UpdatePaydayCredentialsAsync(int userId, string clientId, string clientSecret);
}