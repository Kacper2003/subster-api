using Subster.Models.Dtos;
using Subster.Models.InputModels;

namespace Subster.API.Services.Interfaces;

public interface ITaktikalAuthService
{
    /// <summary>
    /// Authenticates a user via Taktikal and returns the result.
    /// </summary>
    Task<TaktikalAuthResult> AuthenticateAsync(AuthInputModel request);
}
