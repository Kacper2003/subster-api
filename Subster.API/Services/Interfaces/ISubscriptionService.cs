using Subster.Models.Dtos;
using Subster.Models.InputModels;

namespace Subster.API.Services.Interfaces;

public interface ISubscriptionService
{
    Task CreateSubscriptionAsync(SubscriptionInputModel inputModel, string trainerSsn, int clientId, string clientSsn);
    Task<IEnumerable<SubscriptionDto>> GetSubscriptionsAsync(string ssn);
    Task<SubscriptionDetailsDto?> GetSubscriptionByIdAsync(string ssn, int subscriptionId);
}