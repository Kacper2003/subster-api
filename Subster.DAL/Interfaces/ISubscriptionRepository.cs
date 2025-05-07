using Subster.DAL.Entities;
using Subster.Models.Dtos;
using Subster.Models.InputModels;
using Subster.Models.UpdateModels;

namespace Subster.DAL.Interfaces;

public interface ISubscriptionRepository
{
    Task<SubscriptionDetailsDto> CreateSubscriptionAsync(SubscriptionInputModel inputModel, int trainerId, int clientId);
    Task<IEnumerable<SubscriptionDto>> GetAllSubscriptionsAsync(int trainerId);
    Task<IEnumerable<SubscriptionDto>> GetSubscriptionsByClientIdAsync(int clientId);
    Task<SubscriptionDetailsDto?> GetSubscriptionByIdAsync(int trainerId, Guid subscriptionId);
    Task<SubscriptionDetailsDto?> GetClientSubscriptionByIdAsync(int clientId, Guid subscriptionId);
    Task<IEnumerable<Subscription>> GetActiveWithInvoicesAsync(DateTime asOfUtc);
    Task CreateSubscriptionInvoiceAsync(Guid subscriptionId, string paydayInvoiceId, int cycleNumber);
    Task<SubscriptionDetailsDto?> UpdateSubscriptionAsync(Guid subscriptionId, SubscriptionUpdateModel updateModel, int trainerId);
    Task DeactivateSubscriptionAsync(Guid subscriptionId);
}