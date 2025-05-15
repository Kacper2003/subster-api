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

    // Only support 0 for now, as most personal trainers don't have take VAT
    public decimal VatPercentage { get; set; } = 0;
    public bool IsActive { get; set; } = true;
}