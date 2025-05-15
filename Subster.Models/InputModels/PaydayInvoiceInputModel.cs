namespace Subster.Models.InputModels;

public class PaydayInvoiceInputModel
{
    public Customer Customer { get; set; } = null!;
    public string InvoiceDate { get; set; } = null!;
    public string DueDate { get; set; } = null!;
    public string FinalDueDate { get; set; } = null!;

    // Only support ISK for now
    public string CurrencyCode { get; set; } = "ISK";
    public Line[] Lines { get; set; } = null!;
}

public class Customer
{
    public string Id { get; set; } = null!;
}

public class Line
{
    public string Description { get; set; } = null!;
    public int Quantity { get; set; } = 1;
    public decimal UnitPriceExcludingVat { get; set; }
    // public decimal UnitPriceIncludingVat { get; set; }
    public decimal VatPercentage { get; set; } = 0;
    public decimal DiscountPercentage { get; set; } = 0;
}