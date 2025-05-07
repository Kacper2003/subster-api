using Subster.Models.Dtos;
using Subster.Models.InputModels;
using Subster.Models.UpdateModels;

namespace Subster.API.Services.Interfaces;

public interface ISubscriptionService
{
    Task CreateSubscriptionAsync(SubscriptionInputModel inputModel, string trainerSsn, int clientId, string clientSsn);
    Task<IEnumerable<SubscriptionDto>> GetAllSubscriptionsAsync(string ssn);
    Task<SubscriptionDetailsDto> GetSubscriptionByIdAsync(string ssn, Guid subscriptionId);
    Task<SubscriptionDetailsDto> UpdateSubscriptionAsync(string ssn, Guid subscriptionId, SubscriptionUpdateModel inputModel);
    Task DeactivateSubscriptionAsync(string ssn, Guid subscriptionId);
}