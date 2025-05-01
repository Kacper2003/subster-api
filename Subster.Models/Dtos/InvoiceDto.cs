namespace Subster.Models.Dtos;

public class InvoiceDto
{

    public string Description { get; set; } = null!;
    public int InvoiceNumber { get; set; }
    public DateTime InvoiceDate { get; set; }
    public string AmountIncludingVat { get; set; } = null!;
    public string CurrencyCode { get; set; } = null!;
}