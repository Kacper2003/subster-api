namespace Subster.DAL.Entities;

public class Trainer
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Ssn { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public string? PaydayClientId { get; set; }
    public string? PaydayClientSecret { get; set; }

    // Navigation properties
    public ICollection<Program> Programs { get; set; } = [];
    public ICollection<Subscription> Subscriptions { get; set; } = [];
}
