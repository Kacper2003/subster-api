using System.Text.Json.Serialization;

namespace Subster.Models.Dtos.Payday;

public class PaydayInvoice
{
    public string Id { get; set; } = null!;
    public PaydayCustomer Customer { get; set; } = null!;
    public PaydayCustomer Payor { get; set; } = null!;
    public string? Description { get; set; }
    public string? Reference { get; set; }
    public int? Number { get; set; }
    public InvoiceStatus Status { get; set; }
    public DateTime Created { get; set; }
    public DateTime? ClaimCreated { get; set; }
    public DateTime? ClaimFinalDueDate { get; set; }
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime FinalDueDate { get; set; }
    public DateTime? PaidDate { get; set; }
    public DateTime? CancelledDate { get; set; }
    public DateTime? RefundDate { get; set; }
    public DateTime? CreditDate { get; set; }
    public DateTime? SentDate { get; set; }
    public DateTime? ClaimCancelledDate { get; set; }
    public bool ClaimCancelled { get; set; }
    public decimal AmountExcludingVat { get; set; }
    public decimal AmountIncludingVat { get; set; }
    public decimal AmountVat { get; set; }
    public decimal? ForeignAmountExcludingVat { get; set; }
    public decimal? ForeignAmountIncludingVat { get; set; }
    public decimal? ForeignAmountVat { get; set; }
    public string CurrencyCode { get; set; } = null!;
    public decimal CurrencyRate { get; set; }
    public string? VatNumber { get; set; }
    public bool CreateClaim { get; set; }
    public bool CreateElectronicInvoice { get; set; }
    public string? ElectronicInvoicePartyId { get; set; }
    public string? AccountingCost { get; set; }
    public string? Ocr { get; set; }
    public bool SendEmail { get; set; }
    public decimal DefaultInterest { get; set; }
    public decimal CapitalGainsTax { get; set; }
    public List<PaydayInvoiceLine>? Lines { get; set; }
    public string? PaymentType { get; set; }
    public List<PaydayPayment> Payments { get; set; } = new();
    public string? Source { get; set; }
}

public class PaydayCustomer
{
    public string Id { get; set; } = null!;
    public string Language { get; set; } = null!;
    public string Ssn { get; set; } = null!;
    public string? ForeignSsn { get; set; }
    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string ZipCode { get; set; } = null!;
    public string City { get; set; } = null!;
    public string? Country { get; set; }
    public string? Email { get; set; }
    public string? Contact { get; set; }
    public string? Comment { get; set; }
    public string? InvoiceNotes { get; set; }
    public string? AdditionalHeaderInfo { get; set; }
    public decimal? InvoiceLineDiscount { get; set; }
    public string? Phone { get; set; }
    public bool SendElectronicInvoices { get; set; }
    public int? DueDateDefaultDaysAfter { get; set; }
    public int? FinalDueDateDefaultDaysAfter { get; set; }
    public DateTime Created { get; set; }
    public DateTime Edited { get; set; }
    public bool Refetch { get; set; }
}

public class PaydayInvoiceLine
{
    public string? Description { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal VatPercentage { get; set; }
    public string? ProductCode { get; set; }
    public string? Unit { get; set; }
    public decimal? DiscountPercentage { get; set; }
}

public class PaydayPayment
{
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string? PaymentMethod { get; set; }
    public string? TransactionId { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum InvoiceStatus
{
    Draft,
    Sent,
    Paid,
    Cancelled,
    Credited,
    Refunded,
    Overdue
}
