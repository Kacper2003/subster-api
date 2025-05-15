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
    private readonly ITaktikalAuthService _taktikalAuthService;
    private readonly IClientRepository _clientRepository;
    private readonly ITrainerRepository _trainerRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IProgramRepository _programRepository;

    public SubscriptionService(IPaydayService paydayService, ITrainerRepository trainerRepository, ISubscriptionRepository subscriptionRepository, IProgramRepository programRepository, ITaktikalAuthService taktikAuthService, IClientRepository clientRepository)
    {
        _taktikalAuthService = taktikAuthService;
        _clientRepository = clientRepository;
        _paydayService = paydayService;
        _trainerRepository = trainerRepository;
        _subscriptionRepository = subscriptionRepository;
        _programRepository = programRepository;
    }

    public async Task<SubscriptionDetailsDto> CreateSubscriptionAsync(SubscriptionInputModel inputModel, string trainerSsn)
    {
        // 1. Validate trainer exists
        var trainer = await _trainerRepository.GetTrainerBySsnAsync(trainerSsn)
                      ?? throw new UnauthorizedException("Trainer not found");

        // 2. Validate program exists
        var program = await _programRepository.GetProgramByIdAsync(trainer.Id, inputModel.ProgramId)
                      ?? throw new NotFoundException("Program not found");

        // 3. Authenticate client via Taktikal
        var auth = await _taktikalAuthService.AuthenticateAsync(new AuthInputModel
        {
            PhoneNumber = inputModel.ClientPhoneNumber,
            Ssn         = inputModel.ClientSsn
        });

        if (!auth.Authenticated)
            throw new ValidationException($"Client authentication failed: {auth.Error}");

        // 4. Create or fetch local client
        var clientId = await _clientRepository.CreateClientAsync(new UserInputModel
        {
            Name = auth.Customer.Name,
            Ssn  = auth.Customer.Ssn
        });

        // 5. Create subscription record
        var subscription = await _subscriptionRepository.CreateSubscriptionAsync(
            inputModel,
            trainer.Id,
            clientId
        );

        // 6. Issue invoice via Payday
        var invoiceId = await _paydayService.CreateInvoiceAsync(
            trainerSsn,
            auth.Customer.Ssn,
            program
        );

        // 7. Link invoice to subscription
        await _subscriptionRepository.CreateSubscriptionInvoiceAsync(
            subscription.Id,
            invoiceId,
            cycleNumber: 1
        );

        return subscription;
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