using Subster.DAL.Entities;
using Subster.Models.Dtos;
using Subster.Models.InputModels;

namespace Subster.DAL.Interfaces;

public interface ISubscriptionRepository
{
    Task<SubscriptionDto> CreateSubscriptionAsync(SubscriptionInputModel inputModel, int trainerId, int clientId);
    Task<IEnumerable<SubscriptionDto>> GetAllSubscriptionsAsync(int trainerId);
    Task<SubscriptionDetailsDto?> GetSubscriptionByIdAsync(int trainerId, Guid subscriptionId);
    Task<IEnumerable<Subscription>> GetActiveWithInvoicesAsync(DateTime asOfUtc);
    Task CreateSubscriptionInvoiceAsync(Guid subscriptionId, string paydayInvoiceId, int cycleNumber);
    Task DeactivateSubscriptionAsync(Guid subscriptionId);
}