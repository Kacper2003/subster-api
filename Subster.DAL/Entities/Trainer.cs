namespace Subster.DAL.Entities;

public class Trainer
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Ssn { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public string? PaydayClientId { get; set; }
    public string? PaydayClientSecret { get; set; }

    // Navigation properties
    public ICollection<Subscription> Subscriptions { get; set; } = [];
}
