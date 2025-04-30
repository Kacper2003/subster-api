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

    public async Task<int> CreateSubscriptionAsync(SubscriptionInputModel inputModel, int trainerId, int clientId)
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

        return subscription.Id;
    }

    public async Task<IEnumerable<SubscriptionDto>> GetSubscriptionsAsync(int trainerId)
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

    public async Task<SubscriptionDetailsDto?> GetSubscriptionByIdAsync(int trainerId, int subscriptionId)
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

    public async Task CreateSubscriptionInvoiceAsync(int subscriptionId, string invoiceId)
    {
        await _dbContext.SubscriptionInvoices.AddAsync(new SubscriptionInvoice
        {
            SubscriptionId = subscriptionId,
            PaydayInvoiceId = invoiceId
        });
        await _dbContext.SaveChangesAsync();
    }
}