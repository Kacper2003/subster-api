using Subster.DAL.Interfaces;
using Subster.Models.Dtos;
using Microsoft.EntityFrameworkCore;
using Subster.DAL.Entities;


namespace Subster.DAL.Implementations;

public class SubscriptionRepository : ISubscriptionRepository
{
    private readonly SubsterDbContext _dbContext;

    public SubscriptionRepository(SubsterDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task CreateSubscriptionAsync(string clientSsn, string clientName, int userId)
    {
        var subscription = new Subscription
        {
            ClientName = clientName,
            ClientSsn = clientSsn,
            CreatedAt = DateTime.UtcNow,
            UserId = userId
        };

        await _dbContext.Subscriptions.AddAsync(subscription);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<SubscriptionDto>> GetSubscriptionsAsync(int userId)
    {
        var subscriptions = await _dbContext.Subscriptions
            .Where(s => s.UserId == userId)
            .Select(s => new SubscriptionDto
            {
                Id = s.Id,
                ClientName = s.ClientName,
                ClientSsn = s.ClientSsn,
                CreatedAt = s.CreatedAt
            })
            .ToListAsync();

        return subscriptions;
    }
}