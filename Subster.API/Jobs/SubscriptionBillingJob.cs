using Subster.API.Services.Interfaces;

namespace Subster.API.Jobs;

public class SubscriptionBillingJob(ISubscriptionBillingService billingService)
{
    private readonly ISubscriptionBillingService _billingService = billingService;

    // This method is called by Hangfire to execute the job
	public Task ExecuteAsync()
        {
            return _billingService.ProcessDueInvoicesAsync(DateTime.UtcNow);
        }
    }