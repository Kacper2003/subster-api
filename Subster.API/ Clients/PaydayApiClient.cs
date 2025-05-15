using System.Net.Http.Headers;
using Subster.Models.Dtos.Payday;
using Subster.Models.InputModels;

namespace Subster.API.Clients;

public class PaydayApiClient(HttpClient http) : IPaydayApiClient
{
    private readonly HttpClient _http = http;
    private const string AuthPath     = "auth/token";
    private const string CustomersUri = "customers";
    private const string InvoicesUri  = "invoices";

    // Call to Payday API to get an access token
	public async Task<PaydayTokenResponse?> AuthenticateAsync(string clientId, string clientSecret)
    {
		HttpResponseMessage response = await _http.PostAsJsonAsync(AuthPath, new { clientId, clientSecret });
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<PaydayTokenResponse>();
    }

    // Call to Payday API to create a customer or retrieve an existing one
    public async Task<PaydayCustomer?> CreateCustomerAsync(string accessToken, PaydayCustomerInputModel input)
    {
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

		HttpResponseMessage resp = await _http.PostAsJsonAsync(CustomersUri, input);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<PaydayCustomer>();
    }

    // Call to Payday API to create an invoice
    public async Task<PaydayInvoice?> CreateInvoiceAsync(string accessToken, PaydayInvoiceInputModel input)
    {
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

		HttpResponseMessage resp = await _http.PostAsJsonAsync(InvoicesUri, input);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<PaydayInvoice>();
    }
}
