namespace Subster.DAL.Entities;

public class Subscription
{
    public int Id { get; set; }
    public string ClientName { get; set; } = null!;
    public string ClientSsn { get; set; } = null!;
    public DateTime CreatedAt { get; set; }

    // Foreign keys
    public int TrainerId { get; set; }
    public Trainer Trainer { get; set; } = null!;
}