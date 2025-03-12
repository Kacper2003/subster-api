using Subster.Models.Dtos;
using Subster.Models.InputModels;

namespace Subster.API.Services.Interfaces;

public interface ISubscriptionService
{
    Task CreateSubscriptionAsync(SubscriptionInputModel inputModel);
    Task<IEnumerable<SubscriptionDto>> GetSubscriptionsAsync(string ssn);
}