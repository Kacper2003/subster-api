using Subster.API.Services.Interfaces;
using Subster.DAL.Interfaces;
using Subster.Models.Dtos;

namespace Subster.API.Services.Implementations;
public class SubscriptionBillingService(ISubscriptionRepository subscriptionRepository, IPaydayService paydayService) : ISubscriptionBillingService
{
    private readonly ISubscriptionRepository _subscriptionRepository = subscriptionRepository;
    private readonly IPaydayService _paydayService = paydayService;

	public async Task ProcessDueInvoicesAsync(DateTime asOfUtc)
    {
		DateTime today = asOfUtc.Date;

        // Fetch all active subscriptions with their invoices
		IEnumerable<DAL.Entities.Subscription> subscriptions = await _subscriptionRepository.GetActiveWithInvoicesAsync(today);

        // Loop through each subscription
        foreach (DAL.Entities.Subscription s in subscriptions)
        {   
            // If the subscription should have ended, deactivate it
            if (today >= s.EndDate)
            {
                await _subscriptionRepository.DeactivateSubscriptionAsync(s.Id);
                continue;
            }

            // Calculate how many months we should have billed so far
            var monthsElapsed =
                    ((today.Year - s.StartDate.Year) * 12)
                + today.Month
                - s.StartDate.Month
                + 1;

            var maxCycle = Math.Min(monthsElapsed, s.DurationInMonths);

            for (var cycle = 1; cycle <= maxCycle; cycle++)
            {
                // Check if we have already billed for this cycle
                if (s.SubscriptionInvoices.Any(si => si.CycleNumber == cycle))
                    continue;

                // Get the program details
                var programDto = new ProgramDto
                {
                    Id                     = s.Program.Id,
                    Name                   = s.Program.Name,
                    UnitPriceExcludingVat  = s.Program.UnitPriceExcludingVat,
                    VatPercentage          = s.Program.VatPercentage
                };

                // Send the invoice to Payday
                var invoiceId = await _paydayService.CreateInvoiceAsync(
                    trainerSsn: s.Trainer.Ssn,
                    clientSsn:  s.Client.Ssn,
                    program:    programDto);

                // Link the invoice to the subscription
                await _subscriptionRepository.CreateSubscriptionInvoiceAsync(
                    subscriptionId:    s.Id,
                    paydayInvoiceId: invoiceId,
                    cycleNumber:       cycle);
            }
        }
    }
}
