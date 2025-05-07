using System.Net.Http.Headers;
using Subster.Models.Dtos.Payday;
using Subster.Models.InputModels;

namespace Subster.API.Clients;

public class PaydayApiClient : IPaydayApiClient
{
    private readonly HttpClient _http;
    private const string AuthPath     = "auth/token";
    private const string CustomersUri = "customers";
    private const string InvoicesUri  = "invoices";

    public PaydayApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<PaydayTokenResponse?> AuthenticateAsync(string clientId, string clientSecret)
    {
        var response = await _http.PostAsJsonAsync(AuthPath, new { clientId, clientSecret });
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<PaydayTokenResponse>();
    }

    public async Task<PaydayCustomer?> CreateCustomerAsync(string accessToken, PaydayCustomerInputModel input)
    {
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        var resp = await _http.PostAsJsonAsync(CustomersUri, input);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<PaydayCustomer>();
    }

    public async Task<PaydayInvoice?> CreateInvoiceAsync(string accessToken, PaydayInvoiceInputModel input)
    {
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        var resp = await _http.PostAsJsonAsync(InvoicesUri, input);
        Console.WriteLine($"Response: {await resp.Content.ReadAsStringAsync()}");
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<PaydayInvoice>();
    }
}
