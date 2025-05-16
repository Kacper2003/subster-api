using Subster.API.Clients;
using Subster.API.Services.Interfaces;
using Subster.DAL.Interfaces;
using Subster.Models.Dtos.Payday;
using Subster.Models.Dtos;
using Subster.Models.InputModels;
using Subster.API.Exceptions;

namespace Subster.API.Services.Implementations;

public class PaydayService(IPaydayApiClient paydayClient, ITokenService tokenService, ITrainerRepository trainerRepository) : IPaydayService
{
    private readonly IPaydayApiClient _paydayClient = paydayClient;
    private readonly ITokenService _tokenService = tokenService;
    private readonly ITrainerRepository _trainerRepository = trainerRepository;

	public async Task UpdateCredentials(string trainerSsn, string clientId, string clientSecret)
    {
		TrainerDto trainer = await _trainerRepository.GetTrainerBySsnAsync(trainerSsn)
            ?? throw new UnauthorizedException("Invalid trainer credentials.");


        var validCredentials = await _tokenService.ValidateCredentialsAsync(clientId, clientSecret);

        if (!validCredentials)
            throw new UnauthorizedException("Invalid client credentials.");

        await _trainerRepository
            .UpdatePaydayCredentialsAsync(trainer.Id, clientId, clientSecret);
    }

    public async Task DeleteCredentials(string trainerSsn)
    {
		DAL.Entities.Trainer trainer = await _trainerRepository.FindTrainerEntityBySsnAsync(trainerSsn)
            ?? throw new UnauthorizedException("Invalid trainer credentials.");

        if (trainer.PaydayClientId == null || trainer.PaydayClientSecret == null)
            throw new InvalidOperationException("Trainer does not have valid credentials.");
            
        _tokenService.RemoveToken(trainer.Id);
        await _trainerRepository
            .UpdatePaydayCredentialsAsync(trainer.Id, null, null);
    }

    public async Task<string> CreateInvoiceAsync(string trainerSsn, string clientSsn, ProgramDto program)
    {
        // Ensure the trainer exists
		DAL.Entities.Trainer trainer = await _trainerRepository.FindTrainerEntityBySsnAsync(trainerSsn)
                ?? throw new InvalidOperationException("Trainer not found");

        // Get the token for the trainer
        var token = await _tokenService
            .GetTokenAsync(trainer.Id, trainer.PaydayClientId!, trainer.PaydayClientSecret!)
            ?? throw new Exception("Failed to acquire Payday token");

		// Ensure the customer exists (or is created)
		PaydayCustomer customer = await _paydayClient
            .CreateCustomerAsync(token, new PaydayCustomerInputModel { Ssn = clientSsn })
            ?? throw new Exception("Failed to create or retrieve customer");

        var invoiceInput = new PaydayInvoiceInputModel
        {
            Customer     = new Customer { Id = customer.Id },
            InvoiceDate  = DateTime.UtcNow.ToString("yyyy-MM-dd"),
            DueDate      = DateTime.UtcNow.AddDays(3).ToString("yyyy-MM-dd"),
            FinalDueDate = DateTime.UtcNow.AddDays(7).ToString("yyyy-MM-dd"),
            Lines        =
			[
				new Line
                {
                    Description           = program.Name,
                    UnitPriceExcludingVat = program.UnitPriceExcludingVat,
                    VatPercentage         = program.VatPercentage
                }
            ]
        };

		PaydayInvoice invoice = await _paydayClient
            .CreateInvoiceAsync(token, invoiceInput)
            ?? throw new Exception("Failed to create invoice");

        return invoice.Id;
    }
}
