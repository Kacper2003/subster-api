using Subster.Models.Dtos;
using Subster.Models.InputModels;

namespace Subster.DAL.Interfaces;

public interface ISubscriptionRepository
{
    Task<int> CreateSubscriptionAsync(SubscriptionInputModel inputModel, int trainerId, int clientId);
    Task<IEnumerable<SubscriptionDto>> GetSubscriptionsAsync(int trainerId);
    Task<SubscriptionDetailsDto?> GetSubscriptionByIdAsync(int trainerId, int subscriptionId);
    Task CreateSubscriptionInvoiceAsync(int subscriptionId, string paydayInvoiceId);
}