using Subster.API.Services.Interfaces;
using Subster.DAL.Interfaces;
using Subster.Models.Dtos;

namespace Subster.API.Services.Implementations;
public class SubscriptionBillingService : ISubscriptionBillingService
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IPaydayService          _paydayService;

    public SubscriptionBillingService(ISubscriptionRepository subscriptionRepository, IPaydayService paydayService)
    {
        _subscriptionRepository = subscriptionRepository;
        _paydayService = paydayService;
    }

    public async Task ProcessDueInvoicesAsync(DateTime asOfUtc)
    {
        var today = asOfUtc.Date;
        // fetch active subscriptions + their invoice mappings
        var subscriptions = await _subscriptionRepository.GetActiveWithInvoicesAsync(today);

        Console.WriteLine($"Found {subscriptions.Count()} active subscriptions with invoices.");

        foreach (var s in subscriptions)
        {
            var endDate = s.StartDate.AddMonths(s.DurationInMonths).Date;
            if (today >= endDate)
            {
                await _subscriptionRepository.DeactivateSubscriptionAsync(s.Id);
                continue;
            }
            // calculate how many months we should have billed so far
            var monthsElapsed =
                    ((today.Year - s.StartDate.Year) * 12)
                + today.Month
                - s.StartDate.Month
                + 1;

            var maxCycle = Math.Min(monthsElapsed, s.DurationInMonths);

            for (int cycle = 1; cycle <= maxCycle; cycle++)
            {
                // already invoiced?
                if (s.SubscriptionInvoices.Any(si => si.CycleNumber == cycle))
                    continue;

                // send the invoice via your PaydayService
                var programDto = new ProgramDto
                {
                    Id                     = s.Program.Id,
                    Name                   = s.Program.Name,
                    UnitPriceExcludingVat  = s.Program.UnitPriceExcludingVat,
                    VatPercentage          = s.Program.VatPercentage
                };

                var invoiceId = await _paydayService.CreateInvoiceAsync(
                    trainerSsn: s.Trainer.Ssn,
                    clientSsn:  s.Client.Ssn,
                    program:    programDto);

                // record it
                await _subscriptionRepository.CreateSubscriptionInvoiceAsync(
                    subscriptionId:    s.Id,
                    paydayInvoiceId: invoiceId,
                    cycleNumber:       cycle);
            }
        }
    }
}
