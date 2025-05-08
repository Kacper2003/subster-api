namespace Subster.DAL.Entities;

public class Client
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Ssn { get; set; } = null!;
}