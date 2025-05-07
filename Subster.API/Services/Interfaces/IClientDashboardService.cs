using Subster.Models.Dtos;

namespace Subster.API.Services.Interfaces;

public interface IClientDashboardService
{
    Task<IEnumerable<SubscriptionDto>> GetSubscriptionsByClientSsnAsync(string clientSsn);
    Task DeactivateSubscriptionAsync(string clientSsn, Guid subscriptionId);
    Task<IEnumerable<TrainerDto>> GetAllTrainersAsync();
}
