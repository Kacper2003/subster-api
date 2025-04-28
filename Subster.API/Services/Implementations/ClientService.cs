using Subster.API.Services.Interfaces;
using Subster.DAL.Interfaces;
using Subster.Models.Dtos;
using Subster.Models.InputModels;

namespace Subster.API.Services.Implementations;

public class ClientService : IClientService
{
    private readonly IClientRepository _clientRepository;

    public ClientService(IClientRepository clientRepository)
    {
        _clientRepository = clientRepository;
    }

    public async Task<IEnumerable<ClientDto>> GetAllClientsAsync() => await _clientRepository.GetAllClientsAsync();

    public async Task CreateClientIfNotExistsAsync(UserInputModel inputModel)
    {
        var trainer = await _clientRepository.GetClientBySsnAsync(inputModel.Ssn);
        if (trainer == null)
        {
            await _clientRepository.CreateClientAsync(inputModel);
        }
    }
}