namespace Subster.Models.Dtos;

public class ProgramDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal UnitPriceExcludingVat { get; set; }
    public decimal UnitPriceIncludingVat { get; set; }
    public decimal VatPercentage { get; set; }
    public bool IsActive { get; set; } = true;
}
