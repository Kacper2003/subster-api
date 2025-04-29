namespace Subster.DAL.Entities;

public class Subscription
{
    public int Id { get; set; }
    public DateTime StartDate { get; set; }
    public int DurationInMonths { get; set; }
    public DateTime EndDate => StartDate.AddMonths(DurationInMonths);
    public bool IsActive { get; set; } = true;

    // Foreign keys
    public int TrainerId { get; set; }
    public Trainer Trainer { get; set; } = null!;

    public int ClientId { get; set; }
    public Client Client { get; set; } = null!;

    public int ProgramId { get; set; }
    public Program Program { get; set; } = null!;
}