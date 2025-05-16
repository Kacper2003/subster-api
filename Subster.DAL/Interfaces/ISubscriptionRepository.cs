using Subster.DAL.Entities;
using Subster.Models.Dtos;
using Subster.Models.InputModels;
using Subster.Models.UpdateModels;

namespace Subster.DAL.Interfaces;

public interface ISubscriptionRepository
{
    Task<SubscriptionDetailsDto> CreateSubscriptionAsync(SubscriptionInputModel inputModel, Guid trainerId, Guid clientId);
    Task<IEnumerable<SubscriptionDto>> GetAllSubscriptionsAsync(Guid trainerId);
    Task<IEnumerable<SubscriptionDto>> GetSubscriptionsByClientIdAsync(Guid clientId);
    Task<SubscriptionDetailsDto?> GetSubscriptionByIdAsync(Guid trainerId, Guid subscriptionId);
    Task<SubscriptionDetailsDto?> GetClientSubscriptionByIdAsync(Guid clientId, Guid subscriptionId);
    Task<IEnumerable<Subscription>> GetActiveWithInvoicesAsync(DateTime asOfUtc);
    Task CreateSubscriptionInvoiceAsync(Guid subscriptionId, string paydayInvoiceId, int cycleNumber);
    Task<SubscriptionDetailsDto?> UpdateSubscriptionAsync(Guid subscriptionId, SubscriptionUpdateModel updateModel, Guid trainerId);
    Task DeactivateSubscriptionAsync(Guid subscriptionId);
}