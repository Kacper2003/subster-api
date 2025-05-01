using Subster.API.Services.Interfaces;

namespace Subster.API.Jobs;

public class SubscriptionBillingJob
    {
        private readonly ISubscriptionBillingService _billingService;

        public SubscriptionBillingJob(ISubscriptionBillingService billingService)
        {
            _billingService = billingService;
        }

        /// <summary>
        /// This is the method Hangfire will invoke on schedule.
        /// </summary>
        public Task ExecuteAsync()
        {
            // Pass in UtcNow so it picks up today's invoices
            return _billingService.ProcessDueInvoicesAsync(DateTime.UtcNow);
        }
    }