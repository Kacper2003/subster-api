using Subster.Models.InputModels;
using Subster.Models.ResponseModels;

namespace Subster.API.Clients;

public class TaktikalApiClient : ITaktikalApiClient
{
    private readonly HttpClient _http;
    private const string StartPath = "auth/start";
    private const string PollPath  = "auth/poll";

    public TaktikalApiClient(HttpClient http) => _http = http;

    public async Task<StartAuthResponseModel?> StartAsync(StartAuthInputModel dto)
    {
        var resp = await _http.PostAsJsonAsync(StartPath, dto);
        if (!resp.IsSuccessStatusCode) return null;
        Console.WriteLine(await resp.Content.ReadAsStringAsync());
        return await resp.Content.ReadFromJsonAsync<StartAuthResponseModel>();
    }

    public async Task<PollResponseModel?> PollAsync(PollAuthInputModel dto)
    {
        var resp = await _http.PostAsJsonAsync(PollPath, dto);
        if (!resp.IsSuccessStatusCode) return null;
        return await resp.Content.ReadFromJsonAsync<PollResponseModel>();
    }
}
