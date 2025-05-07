using Subster.API.Services.Interfaces;
using Subster.DAL.Interfaces;
using Subster.Models.Dtos;
using Subster.API.Exceptions;

namespace Subster.API.Services.Implementations;

public class ClientDashboardService : IClientDashboardService
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly ITrainerRepository _trainerRepository;
    private readonly IClientRepository _clientRepository;

    public ClientDashboardService(
        ISubscriptionRepository subscriptionRepository,
        ITrainerRepository trainerRepository,
        IClientRepository clientRepository)
    {
        _subscriptionRepository = subscriptionRepository;
        _trainerRepository = trainerRepository;
        _clientRepository = clientRepository;
    }

    public async Task<IEnumerable<SubscriptionDto>> GetSubscriptionsByClientSsnAsync(string clientSsn)
    {
        var client = await _clientRepository.GetClientBySsnAsync(clientSsn)
            ?? throw new UnauthorizedException($"Invalid client credentials.");
        
        return await _subscriptionRepository.GetSubscriptionsByClientIdAsync(client.Id);
    }

    public async Task DeactivateSubscriptionAsync(string clientSsn, Guid subscriptionId)
    {
        var client = await _clientRepository.GetClientBySsnAsync(clientSsn)
            ?? throw new UnauthorizedException($"Invalid client credentials.");

        var existingSubscription = await _subscriptionRepository.GetClientSubscriptionByIdAsync(client.Id, subscriptionId)
            ?? throw new NotFoundException($"Subscription with id {subscriptionId} not found.");

        await _subscriptionRepository.DeactivateSubscriptionAsync(subscriptionId);
    }

    public async Task<IEnumerable<TrainerDto>> GetAllTrainersAsync() => await _trainerRepository.GetAllTrainersAsync();
}
