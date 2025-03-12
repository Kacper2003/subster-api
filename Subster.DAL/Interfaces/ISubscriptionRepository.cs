using Subster.Models.Dtos;
using Subster.Models.InputModels;

namespace Subster.DAL.Interfaces;

public interface ISubscriptionRepository
{
    Task CreateSubscriptionAsync(string clientSsn, string clientName, int userId);
    Task<IEnumerable<SubscriptionDto>> GetSubscriptionsAsync(int userId);
}