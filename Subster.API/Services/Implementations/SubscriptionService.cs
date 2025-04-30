using Subster.DAL.Interfaces;
using Subster.API.Services.Interfaces;
using Subster.Models.InputModels;
using Subster.Models.Dtos;

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

    public async Task CreateSubscriptionAsync(SubscriptionInputModel inputModel, string trainerSsn, int clientId, string clientSsn)
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

        var subscriptionId = await _subscriptionRepository.CreateSubscriptionAsync(inputModel, trainer.Id, clientId);

        var paydayInvoiceId = await _paydayService.CreateInvoiceAsync(trainerSsn, clientSsn, program);

        await _subscriptionRepository.CreateSubscriptionInvoiceAsync(subscriptionId, paydayInvoiceId);
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

    public async Task<SubscriptionDetailsDto?> GetSubscriptionByIdAsync(string ssn, int subscriptionId)
    {
        var trainer = await _trainerRepository.GetTrainerBySsnAsync(ssn);
        if (trainer == null)
        {
            throw new Exception("Trainer not found");
        }

        return await _subscriptionRepository.GetSubscriptionByIdAsync(trainer.Id, subscriptionId);
    }
}