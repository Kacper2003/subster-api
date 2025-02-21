using Subster.Models.Dtos;

namespace Subster.API.Services.Interfaces;

public interface IUserService
{
    Task<IEnumerable<UserDto>> GetAllUsersAsync();

}