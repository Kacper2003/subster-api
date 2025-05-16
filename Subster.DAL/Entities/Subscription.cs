namespace Subster.DAL.Entities;

public class Subscription
{
    public Guid Id { get; set; }
    public Guid TrainerId { get; set; }
    public Guid ClientId { get; set; }
    public Guid ProgramId { get; set; }
    public DateTime StartDate { get; set; }
    public int DurationInMonths { get; set; }
    public DateTime EndDate => StartDate.AddMonths(DurationInMonths);
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public Trainer Trainer { get; set; } = null!;
    public Client Client { get; set; } = null!;
    public Program Program { get; set; } = null!;
    public ICollection<SubscriptionInvoice> SubscriptionInvoices { get; set; } = [];
}