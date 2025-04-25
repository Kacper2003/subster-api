namespace Subster.DAL.Entities;

public class Subscription
{
    public int Id { get; set; }
    public string ClientSsn { get; set; } = null!;
    public DateTime CreatedAt { get; set; }

    // Foreign keys
    public int TrainerId { get; set; }
    public Trainer Trainer { get; set; } = null!;

    public int ClientId { get; set; }
    public Client Client { get; set; } = null!;

    public int ProgramId { get; set; }
    public Program Program { get; set; } = null!;
}