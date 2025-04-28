using Subster.DAL.Interfaces;
using Subster.API.Services.Interfaces;
using Subster.Models.InputModels;
using Subster.Models.Dtos;

namespace Subster.API.Services.Implementations;

public class SubscriptionService : ISubscriptionService
{
    private readonly ITrainerRepository _trainerRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;

    public SubscriptionService(ITrainerRepository trainerRepository, ISubscriptionRepository subscriptionRepository)
    {
        _trainerRepository = trainerRepository;
        _subscriptionRepository = subscriptionRepository;
    }

    public async Task CreateSubscriptionAsync(SubscriptionInputModel inputModel)
    {
        var trainer = await _trainerRepository.GetTrainerBySsnAsync(inputModel.TrainerSsn);
        if (trainer == null)
        {
            throw new Exception("Trainer not found");
        }

        await _subscriptionRepository.CreateSubscriptionAsync(inputModel.ClientSsn, inputModel.ClientName, trainer.Id);
    }

    public async Task<IEnumerable<SubscriptionDto>> GetSubscriptionsAsync(string ssn)
    {
        var trainer = await _trainerRepository.GetTrainerBySsnAsync(ssn);
        if (trainer == null)
        {
            throw new Exception("Trainer not found");
        }

        return await _subscriptionRepository.GetSubscriptionsAsync(trainer.Id);
    }
}