using Subster.Models.Dtos;
using Subster.Models.InputModels;
using Subster.Models.UpdateModels;

namespace Subster.API.Services.Interfaces;

public interface ISubscriptionService
{
    Task CreateSubscriptionAsync(SubscriptionInputModel inputModel, string trainerSsn, Guid clientId, string clientSsn);
    Task<IEnumerable<SubscriptionDto>> GetAllSubscriptionsAsync(string trainerSsn);
    Task<SubscriptionDetailsDto> GetSubscriptionByIdAsync(string trainerSsn, Guid subscriptionId);
    Task<SubscriptionDetailsDto> UpdateSubscriptionAsync(string trainerSsn, Guid subscriptionId, SubscriptionUpdateModel inputModel);
    Task DeactivateSubscriptionAsync(string trainerSsn, Guid subscriptionId);
}