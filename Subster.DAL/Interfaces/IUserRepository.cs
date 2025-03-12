using Subster.Models.Dtos;
using Subster.Models.InputModels;

namespace Subster.DAL.Interfaces;

public interface IUserRepository
{
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
    Task<UserDto?> GetUserBySsnAsync(string ssn);
    Task CreateUserAsync(UserInputModel inputModel);
}