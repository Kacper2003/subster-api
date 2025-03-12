namespace Subster.DAL.Entities;

public class Subscription
{
    public int Id { get; set; }
    public string ClientName { get; set; } = null!;
    public string ClientSsn { get; set; } = null!;
    public DateTime CreatedAt { get; set; }

    // Foreign keys
    public int UserId { get; set; }
    public User User { get; set; } = null!;
}