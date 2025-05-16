namespace Subster.Models.Dtos;

public class TrainerDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string Ssn { get; set; } = "";
    public string? PhoneNumber { get; set; }
}
