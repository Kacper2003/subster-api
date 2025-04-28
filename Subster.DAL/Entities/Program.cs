namespace Subster.DAL.Entities;

public class Program
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public int Price { get; set; }
    public int DurationInMonths { get; set; }
}