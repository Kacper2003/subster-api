namespace Subster.Models.InputModels;

public class InvoiceInputModel
{
    public Customer Customer { get; set; } = null!;
    public string Ssn { get; set; } = null!;
    public string InvoiceDate { get; set; } = null!;
    public string DueDate { get; set; } = null!;
    public string FinalDueDate { get; set; } = null!;
    // Default value is "ISK" (atleast for now)
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
    public int Quantity { get; set; }
    public decimal UnitPriceExcludingVat { get; set; }
    public decimal UnitPriceIncludingVat { get; set; }
    public decimal VarPercentage { get; set; } = 0;
    public decimal DiscountPercentage { get; set; } = 0;

}