using Subster.Models.InputModels;
using Subster.Models.ResponseModels;

namespace Subster.API.Services.Interfaces;

public interface ITaktikalAuthService
{
    /// <summary>
    /// Authenticates a user via Taktikal and returns the result.
    /// </summary>
    Task<EndAuthResponseModel> AuthenticateAsync(AuthInputModel request);
}
