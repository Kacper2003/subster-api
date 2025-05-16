using Subster.DAL.Interfaces;
using Subster.API.Services.Interfaces;
using Subster.Models.InputModels;
using Subster.Models.Dtos;
using Subster.Models.UpdateModels;
using Subster.API.Exceptions;

namespace Subster.API.Services.Implementations;

public class SubscriptionService(IPaydayService paydayService, ITrainerRepository trainerRepository, ISubscriptionRepository subscriptionRepository, IProgramRepository programRepository, ITaktikalAuthService taktikAuthService, IClientRepository clientRepository) : ISubscriptionService
{
    private readonly IPaydayService _paydayService = paydayService;
    private readonly ITaktikalAuthService _taktikalAuthService = taktikAuthService;
    private readonly IClientRepository _clientRepository = clientRepository;
    private readonly ITrainerRepository _trainerRepository = trainerRepository;
    private readonly ISubscriptionRepository _subscriptionRepository = subscriptionRepository;
    private readonly IProgramRepository _programRepository = programRepository;

	public async Task<SubscriptionDetailsDto> CreateSubscriptionAsync(SubscriptionInputModel inputModel, string trainerSsn)
    {
		TrainerDto trainer = await _trainerRepository.GetTrainerBySsnAsync(trainerSsn)
                      ?? throw new UnauthorizedException("Trainer not found");

		ProgramDto program = await _programRepository.GetProgramByIdAsync(trainer.Id, inputModel.ProgramId)
                      ?? throw new NotFoundException("Program not found");

        // Get client confirmation on the subscription
		Models.ResponseModels.EndAuthResponseModel auth = await _taktikalAuthService.AuthenticateAsync(new AuthInputModel
        {
            PhoneNumber = inputModel.ClientPhoneNumber,
            Ssn         = inputModel.ClientSsn
        });

        if (!auth.Authenticated)
            throw new ValidationException($"Client authentication failed: {auth.Error}");

		// Create a Client record, so we can link it to the subscription, and the client can log in and manage their subscription
		Guid clientId = await _clientRepository.CreateClientAsync(new UserInputModel
        {
            Name = auth.Customer.Name,
            Ssn  = auth.Customer.Ssn
        });

        // 
		SubscriptionDetailsDto subscription = await _subscriptionRepository.CreateSubscriptionAsync(
            inputModel,
            trainer.Id,
            clientId
        );

        // Even if it fails, the cron job will send the invoice a day later
        var invoiceId = await _paydayService.CreateInvoiceAsync(
            trainerSsn,
            auth.Customer.Ssn,
            program
        );

        await _subscriptionRepository.CreateSubscriptionInvoiceAsync(
            subscription.Id,
            invoiceId,
            cycleNumber: 1
        );

        return subscription;
    }

    public async Task<SubscriptionDetailsDto> UpdateSubscriptionAsync(string trainerSsn, Guid subscriptionId, SubscriptionUpdateModel updateModel)
    {
		TrainerDto trainer = await _trainerRepository.GetTrainerBySsnAsync(trainerSsn)
            ?? throw new UnauthorizedException("Invalid trainer credentials.");

		SubscriptionDetailsDto updatedSubscription = await _subscriptionRepository.UpdateSubscriptionAsync(subscriptionId, updateModel, trainer.Id)
            ?? throw new NotFoundException($"Subscription with id {subscriptionId} not found.");

        return updatedSubscription;
    }

    public async Task<IEnumerable<SubscriptionDto>> GetAllSubscriptionsAsync(string trainerSsn)
    {
		TrainerDto? trainer = await _trainerRepository.GetTrainerBySsnAsync(trainerSsn) 
            ?? throw new Exception("Trainer not found");
            
		return await _subscriptionRepository.GetAllSubscriptionsAsync(trainer.Id);
    }

    public async Task<SubscriptionDetailsDto> GetSubscriptionByIdAsync(string trainerSsn, Guid subscriptionId)
    {
		TrainerDto trainer = await _trainerRepository.GetTrainerBySsnAsync(trainerSsn)
            ?? throw new UnauthorizedException("Invalid trainer credentials.");

		SubscriptionDetailsDto subscription = await _subscriptionRepository.GetSubscriptionByIdAsync(trainer.Id, subscriptionId)
            ?? throw new NotFoundException($"Subscription with id {subscriptionId} not found.");

        return subscription;
    }

    // 
    public async Task DeactivateSubscriptionAsync(string trainerSsn, Guid subscriptionId)
    {
		TrainerDto trainer = await _trainerRepository.GetTrainerBySsnAsync(trainerSsn)
            ?? throw new UnauthorizedException("Invalid trainer credentials.");

		SubscriptionDetailsDto existingSubscription = await _subscriptionRepository.GetSubscriptionByIdAsync(trainer.Id, subscriptionId)
            ?? throw new NotFoundException($"Subscription with id {subscriptionId} not found.");

        await _subscriptionRepository.DeactivateSubscriptionAsync(subscriptionId);
    }
}