namespace Subster.Models.UpdateModels;

public class ProgramUpdateModel
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? UnitPriceExcludingVat { get; set; }
    public decimal? UnitPriceIncludingVat { get; set; }
    public decimal? VatPercentage { get; set; }
}