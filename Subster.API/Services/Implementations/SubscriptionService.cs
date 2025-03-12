using Subster.DAL.Interfaces;
using Subster.API.Services.Interfaces;
using Subster.Models.InputModels;
using Subster.Models.Dtos;

namespace Subster.API.Services.Implementations;

public class SubscriptionService : ISubscriptionService
{
    private readonly IUserRepository _userRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;

    public SubscriptionService(IUserRepository userRepository, ISubscriptionRepository subscriptionRepository)
    {
        _userRepository = userRepository;
        _subscriptionRepository = subscriptionRepository;
    }

    public async Task CreateSubscriptionAsync(SubscriptionInputModel inputModel)
    {
        var user = await _userRepository.GetUserBySsnAsync(inputModel.UserSsn);
        if (user == null)
        {
            throw new Exception("User not found");
        }

        await _subscriptionRepository.CreateSubscriptionAsync(inputModel.ClientSsn, inputModel.ClientName, user.Id);
    }

    public async Task<IEnumerable<SubscriptionDto>> GetSubscriptionsAsync(string ssn)
    {
        var user = await _userRepository.GetUserBySsnAsync(ssn);
        if (user == null)
        {
            throw new Exception("User not found");
        }

        return await _subscriptionRepository.GetSubscriptionsAsync(user.Id);
    }
}