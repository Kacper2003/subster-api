using Subster.Models.Dtos;

namespace Subster.API.Services.Interfaces;

public interface IPaydayService
{
    /// <summary>
    /// Stores and validates the trainer’s Payday credentials.
    /// </summary>
    Task UpdateCredentials(string ssn, string clientId, string clientSecret);

    /// <summary>
    /// Removes the trainer’s stored Payday credentials.
    /// </summary>
    Task DeleteCredentials(string ssn);

    /// <summary>
    /// Creates (or finds) a customer and then issues an invoice, returning its ID.
    /// </summary>
    Task<string> CreateInvoiceAsync(string trainerSsn, string clientSsn, ProgramDto program);
}
