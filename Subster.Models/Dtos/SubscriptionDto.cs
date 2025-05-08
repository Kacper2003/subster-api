namespace Subster.Models.Dtos;

public class SubscriptionDto
{
    public Guid Id { get; set; }
    public string TrainerName { get; set; } = null!;
    public string ClientName { get; set; } = null!;
    public string ProgramName { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int DurationInMonths { get; set; }
}