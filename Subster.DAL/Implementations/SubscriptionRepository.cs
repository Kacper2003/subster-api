using Subster.DAL.Interfaces;
using Subster.Models.Dtos;
using Microsoft.EntityFrameworkCore;
using Subster.DAL.Entities;
using Subster.Models.InputModels;


namespace Subster.DAL.Implementations;

public class SubscriptionRepository : ISubscriptionRepository
{
    private readonly SubsterDbContext _dbContext;

    public SubscriptionRepository(SubsterDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SubscriptionDto> CreateSubscriptionAsync(SubscriptionInputModel inputModel, int trainerId, int clientId)
    {
        var subscription = new Subscription
        {
            // First foreign keys
            TrainerId           = trainerId,
            ClientId            = clientId,
            ProgramId           = inputModel.ProgramId,

            StartDate           = inputModel.StartDate,
            DurationInMonths    = inputModel.DurationInMonths,
            IsActive            = true
        };

        await _dbContext.Subscriptions.AddAsync(subscription);
        await _dbContext.SaveChangesAsync();

        return new SubscriptionDto
        {
            Id                  = subscription.Id,
            ClientName          = subscription.Client.Name,
            ProgramName         = subscription.Program.Name,
            StartDate           = subscription.StartDate,
            EndDate             = subscription.EndDate,
            DurationInMonths    = subscription.DurationInMonths
        };
    }

    public async Task<IEnumerable<SubscriptionDto>> GetAllSubscriptionsAsync(int trainerId)
    {
        return await _dbContext.Subscriptions
            .Include(s => s.Client)
            .Include(s => s.Program)
            .Where(s => s.TrainerId == trainerId)
            .Select(s => new SubscriptionDto
            {
                Id                  = s.Id,
                ClientName          = s.Client.Name,
                ProgramName         = s.Program.Name,
                StartDate           = s.StartDate,
                EndDate             = s.EndDate,
                DurationInMonths    = s.DurationInMonths
            })
            .ToListAsync();
        
    }

    public async Task<SubscriptionDetailsDto?> GetSubscriptionByIdAsync(int trainerId, Guid subscriptionId)
    {
        return await _dbContext.Subscriptions
            .Include(s => s.Client)
            .Include(s => s.Program)
            .Where(s => s.TrainerId == trainerId && s.Id == subscriptionId)
            .Select(s => new SubscriptionDetailsDto
            {
                Id                  = s.Id,
                ClientName          = s.Client.Name,
                Program             = new ProgramDto
                                    {
                                        Id = s.Program.Id,
                                        Name = s.Program.Name,
                                        Description = s.Program.Description,
                                        UnitPriceExcludingVat = s.Program.UnitPriceExcludingVat,
                                        UnitPriceIncludingVat = s.Program.UnitPriceIncludingVat,
                                        VatPercentage = s.Program.VatPercentage,
                                    },
                StartDate           = s.StartDate,
                EndDate             = s.EndDate,
                DurationInMonths    = s.DurationInMonths
            }).FirstOrDefaultAsync();
    }

    public async Task CreateSubscriptionInvoiceAsync(Guid subscriptionId, string invoiceId, int cycleNumber)
    {
        await _dbContext.SubscriptionInvoices.AddAsync(new SubscriptionInvoice
        {
            SubscriptionId = subscriptionId,
            CycleNumber = cycleNumber,
            PaydayInvoiceId = invoiceId,
            SentAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<Subscription>> GetActiveWithInvoicesAsync(DateTime asOfUtc)
    {
        var date = asOfUtc.Date;

        return await _dbContext.Subscriptions
            .Include(s => s.Trainer)
            .Include(s => s.Client)
            .Include(s => s.Program)
            .Include(s => s.SubscriptionInvoices)
            .Where(s =>
                s.IsActive
                && s.StartDate.Date <= date
            )
            .ToListAsync();
    }

    public async Task DeactivateSubscriptionAsync(Guid subscriptionId)
    {
        var subscription = await _dbContext.Subscriptions
            .FirstOrDefaultAsync(s => s.Id == subscriptionId);

        if (subscription != null)
        {
            if (!subscription.IsActive) throw new InvalidOperationException("Subscription is already inactive.");
            subscription.IsActive = false;
            await _dbContext.SaveChangesAsync();
        }
    }
}
