namespace Subster.DAL.Entities;

public class SubscriptionInvoice
{
    public Guid Id { get; set; }
    public Guid SubscriptionId { get; set; }
    public string PaydayInvoiceId { get; set; } = null!;
    public int CycleNumber { get; set; }
    public DateTime SentAt { get; set; }
    
    // Navigation properties
    public Subscription Subscription { get; set; } = null!;
}