using Subster.Models.Dtos;
using Subster.Models.InputModels;

namespace Subster.API.Services.Interfaces;

public interface IClientService
{
    Task<IEnumerable<ClientDto>> GetAllClientsAsync();
    Task CreateClientIfNotExistsAsync(UserInputModel inputModel);
}