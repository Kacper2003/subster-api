namespace Subster.API.Services.Interfaces;

/// <summary>
/// Processes due subscription invoices (initial + recurring).
/// </summary>
public interface ISubscriptionBillingService
{
    /// <summary>
    /// Scan all active subscriptions and send any missing invoices up through asOfUtc.
    /// </summary>
    Task ProcessDueInvoicesAsync(DateTime asOfUtc);
}
