using Subster.Models.Dtos;
using Subster.Models.InputModels;

namespace Subster.API.Services.Interfaces;

public interface IPaydayService
{
    Task<bool> UpdateCredentials(string Ssn, string clientId, string clientSecret);
    Task<bool> DeleteCredentials(string ssn);
    Task<string?> GetAccessToken(string ssn, string? newClientId = null, string? newClientSecret = null);
}