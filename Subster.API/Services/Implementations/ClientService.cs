using Subster.API.Services.Interfaces;
using Subster.DAL.Interfaces;
using Subster.Models.Dtos;
using Subster.Models.InputModels;

namespace Subster.API.Services.Implementations;

public class ClientService(IClientRepository clientRepository) : IClientService
{
    private readonly IClientRepository _clientRepository = clientRepository;

	public async Task<IEnumerable<ClientDto>> GetAllClientsAsync() => await _clientRepository.GetAllClientsAsync();

    public async Task<Guid> CreateClientIfNotExistsAsync(UserInputModel inputModel)
    {
		ClientDto? client = await _clientRepository.GetClientBySsnAsync(inputModel.Ssn);
        if (client == null)
        {
			Guid clientId = await _clientRepository.CreateClientAsync(inputModel);
            return clientId;
        }
        return client.Id;
    }
}