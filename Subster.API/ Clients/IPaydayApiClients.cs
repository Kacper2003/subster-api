using Subster.Models.Dtos.Payday;
using Subster.Models.InputModels;

namespace Subster.API.Clients;

public interface IPaydayApiClient
{
    Task<PaydayTokenResponse?> AuthenticateAsync(string clientId, string clientSecret);
    Task<PaydayCustomer?> CreateCustomerAsync(string accessToken, PaydayCustomerInputModel input);
    Task<PaydayInvoice?> CreateInvoiceAsync(string accessToken, PaydayInvoiceInputModel input);
}

