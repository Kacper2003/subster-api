using System.ComponentModel.DataAnnotations;

namespace Subster.Models.InputModels;

public class ProgramInputModel
{
    [Required]
    public string Name { get; set; } = null!;
    [Required]
    public string Description { get; set; } = null!;
    public decimal? UnitPriceExcludingVat { get; set; }
    public decimal? UnitPriceIncludingVat { get; set; }
    public decimal VatPercentage { get; set; } = 24;
    public bool IsActive { get; set; } = true;
}