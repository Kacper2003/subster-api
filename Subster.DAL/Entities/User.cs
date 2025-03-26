namespace Subster.DAL.Entities;
// Base user í bili, líklegast splittað í einkaþjálfara og notanda
public class User
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
