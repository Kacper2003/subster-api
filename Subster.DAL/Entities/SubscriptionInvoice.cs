namespace Subster.DAL.Entities;

public class SubscriptionInvoice
{
    public int Id { get; set; }
    public int SubscriptionId { get; set; }
    public Subscription Subscription { get; set; } = null!;
    public string PaydayInvoiceId { get; set; } = null!;
}