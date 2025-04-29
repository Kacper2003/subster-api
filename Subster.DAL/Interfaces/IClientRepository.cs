using Subster.Models.Dtos;
using Subster.Models.InputModels;

namespace Subster.DAL.Interfaces;

public interface IClientRepository
{
    Task<IEnumerable<ClientDto>> GetAllClientsAsync();
    Task<ClientDto?> GetClientBySsnAsync(string ssn);
    Task<int> CreateClientAsync(UserInputModel inputModel);
}