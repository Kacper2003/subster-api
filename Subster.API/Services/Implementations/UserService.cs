using Subster.API.Services.Interfaces;
using Subster.DAL.Interfaces;
using Subster.Models.Dtos;

namespace Subster.API.Services.Implementations;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        return await _userRepository.GetAllUsersAsync();
    }
}