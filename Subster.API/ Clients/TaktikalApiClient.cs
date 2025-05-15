using Subster.Models.InputModels;
using Subster.Models.ResponseModels;

namespace Subster.API.Clients;

public class TaktikalApiClient(HttpClient http) : ITaktikalApiClient
{
    private readonly HttpClient _http = http;
    private const string StartPath = "auth/start";
    private const string PollPath  = "auth/poll";

    // Call to Taktikal API to start the authentication process
	public async Task<StartAuthResponseModel?> StartAsync(StartAuthInputModel dto)
    {
		HttpResponseMessage resp = await _http.PostAsJsonAsync(StartPath, dto);
        if (!resp.IsSuccessStatusCode) return null;
        return await resp.Content.ReadFromJsonAsync<StartAuthResponseModel>();
    }

    // Call to Taktikal API to poll the authentication status
    public async Task<PollResponseModel?> PollAsync(PollAuthInputModel dto)
    {
		HttpResponseMessage resp = await _http.PostAsJsonAsync(PollPath, dto);
        if (!resp.IsSuccessStatusCode) return null;
        return await resp.Content.ReadFromJsonAsync<PollResponseModel>();
    }
}
