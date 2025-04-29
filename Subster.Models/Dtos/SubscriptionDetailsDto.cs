namespace Subster.Models.Dtos;

public class SubscriptionDetailsDto
{
    public int Id { get; set; }
    public string ClientName { get; set; } = null!;
    public ProgramDto Program { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int DurationInMonths { get; set; }    
}