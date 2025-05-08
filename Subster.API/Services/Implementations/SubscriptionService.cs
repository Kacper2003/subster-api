using Subster.DAL.Interfaces;
using Subster.API.Services.Interfaces;
using Subster.Models.InputModels;
using Subster.Models.Dtos;
using Subster.Models.UpdateModels;
using Subster.API.Exceptions;

namespace Subster.API.Services.Implementations;

public class SubscriptionService : ISubscriptionService
{
    private readonly IPaydayService _paydayService;
    private readonly ITrainerRepository _trainerRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IProgramRepository _programRepository;

    public SubscriptionService(IPaydayService paydayService, ITrainerRepository trainerRepository, ISubscriptionRepository subscriptionRepository, IProgramRepository programRepository)
    {
        _paydayService = paydayService;
        _trainerRepository = trainerRepository;
        _subscriptionRepository = subscriptionRepository;
        _programRepository = programRepository;
    }

    public async Task CreateSubscriptionAsync(SubscriptionInputModel inputModel, string trainerSsn, Guid clientId, string clientSsn)
    {
        
        var trainer = await _trainerRepository.GetTrainerBySsnAsync(trainerSsn);
        if (trainer == null)
        {
            throw new Exception("Trainer not found");
        }

        var program = await _programRepository.GetProgramByIdAsync(trainer.Id, inputModel.ProgramId);
        if (program == null)
        {
            throw new Exception("Program not found");
        }

        // Create the subscription
        var subscription = await _subscriptionRepository.CreateSubscriptionAsync(inputModel, trainer.Id, clientId);

        // Immediately create the invoice
        var paydayInvoiceId = await _paydayService.CreateInvoiceAsync(trainerSsn, clientSsn, program);

        // Create the invoice, with the cycle set to 1 (guaranteed to have cycle 1)
        await _subscriptionRepository.CreateSubscriptionInvoiceAsync(subscription.Id, paydayInvoiceId, 1);
    }

    public async Task<SubscriptionDetailsDto> UpdateSubscriptionAsync(string trainerSsn, Guid subscriptionId, SubscriptionUpdateModel updateModel)
    {
        var trainer = await _trainerRepository.GetTrainerBySsnAsync(trainerSsn)
            ?? throw new UnauthorizedException("Invalid trainer credentials.");

        var updatedSubscription = await _subscriptionRepository.UpdateSubscriptionAsync(subscriptionId, updateModel, trainer.Id)
            ?? throw new NotFoundException($"Subscription with id {subscriptionId} not found.");

        return updatedSubscription;
    }

    public async Task<IEnumerable<SubscriptionDto>> GetAllSubscriptionsAsync(string trainerSsn)
    {
        var trainer = await _trainerRepository.GetTrainerBySsnAsync(trainerSsn);
        if (trainer == null)
        {
            throw new Exception("Trainer not found");
        }

        return await _subscriptionRepository.GetAllSubscriptionsAsync(trainer.Id);
    }

    public async Task<SubscriptionDetailsDto> GetSubscriptionByIdAsync(string trainerSsn, Guid subscriptionId)
    {
        var trainer = await _trainerRepository.GetTrainerBySsnAsync(trainerSsn)
            ?? throw new UnauthorizedException("Invalid trainer credentials.");        

        var subscription = await _subscriptionRepository.GetSubscriptionByIdAsync(trainer.Id, subscriptionId)
            ?? throw new NotFoundException($"Subscription with id {subscriptionId} not found.");

        return subscription;
    }

    public async Task DeactivateSubscriptionAsync(string trainerSsn, Guid subscriptionId)
    {
        var trainer = await _trainerRepository.GetTrainerBySsnAsync(trainerSsn)
            ?? throw new UnauthorizedException("Invalid trainer credentials.");

        var existingSubscription = await _subscriptionRepository.GetSubscriptionByIdAsync(trainer.Id, subscriptionId)
            ?? throw new NotFoundException($"Subscription with id {subscriptionId} not found.");

        await _subscriptionRepository.DeactivateSubscriptionAsync(subscriptionId);
    }
}