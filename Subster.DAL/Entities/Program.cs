namespace Subster.DAL.Entities;

public class Program
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public decimal UnitPriceExcludingVat { get; set; }
    public decimal UnitPriceIncludingVat { get; set; }
    public decimal VatPercentage { get; set; }
    public bool IsActive { get; set; } = true;

    // Foreign keys
    public Guid TrainerId { get; set; }
    public Trainer Trainer { get; set; } = null!;
    public ICollection<Subscription> Subscriptions { get; set; } = [];
}