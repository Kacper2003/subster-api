using Subster.DAL.Interfaces;
using Subster.Models.Dtos;
using Microsoft.EntityFrameworkCore;
using Subster.DAL.Entities;
using Subster.Models.InputModels;
using Subster.Models.UpdateModels;


namespace Subster.DAL.Implementations;

public class SubscriptionRepository(SubsterDbContext dbContext) : ISubscriptionRepository
{
    private readonly SubsterDbContext _dbContext = dbContext;

	public async Task<SubscriptionDetailsDto> CreateSubscriptionAsync(SubscriptionInputModel inputModel, Guid trainerId, Guid clientId)
    {
        // Service already validates the input model
        var subscription = new Subscription
        {
            TrainerId           = trainerId,
            ClientId            = clientId,
            ProgramId           = inputModel.ProgramId,
            StartDate           = inputModel.StartDate,
            DurationInMonths    = inputModel.DurationInMonths,
            IsActive            = true
        };

        await _dbContext.Subscriptions.AddAsync(subscription);
        await _dbContext.SaveChangesAsync();

		Subscription? savedSubscription = await _dbContext.Subscriptions
            .Include(s => s.Client)
            .Include(s => s.Program)
            .FirstOrDefaultAsync(s => s.Id == subscription.Id);

        return new SubscriptionDetailsDto
        {
            Id                  = savedSubscription!.Id,
            ClientName          = savedSubscription.Client.Name,
            Program             = new ProgramDto
                                    {
                                        Id = savedSubscription.Program.Id,
                                        Name = savedSubscription.Program.Name,
                                        Description = savedSubscription.Program.Description,
                                        UnitPriceExcludingVat = savedSubscription.Program.UnitPriceExcludingVat,
                                        UnitPriceIncludingVat = savedSubscription.Program.UnitPriceIncludingVat,
                                        VatPercentage = savedSubscription.Program.VatPercentage,
                                    },
            StartDate           = savedSubscription.StartDate,
            EndDate             = savedSubscription.EndDate,
            DurationInMonths    = savedSubscription.DurationInMonths
        };
    }

    public async Task<IEnumerable<SubscriptionDto>> GetAllSubscriptionsAsync(Guid trainerId)
    {
        return await _dbContext.Subscriptions
            .Include(s => s.Client)
            .Include(s => s.Program)
            .Include(s => s.Trainer)
            .Where(s => s.TrainerId == trainerId)
            .Select(s => new SubscriptionDto
            {
                Id                  = s.Id,
                TrainerName         = s.Trainer.Name,
                ClientName          = s.Client.Name,
                ProgramName         = s.Program.Name,
                StartDate           = s.StartDate,
                EndDate             = s.EndDate,
                DurationInMonths    = s.DurationInMonths
            })
            .ToListAsync();
        
    }

    public async Task<IEnumerable<SubscriptionDto>> GetSubscriptionsByClientIdAsync(Guid clientId)
    {
        return await _dbContext.Subscriptions
            .Include(s => s.Client)
            .Include(s => s.Program)
            .Include(s => s.Trainer)
            .Where(s => s.ClientId == clientId && s.IsActive)
            .Select(s => new SubscriptionDto
            {
                Id                  = s.Id,
                TrainerName         = s.Trainer.Name,
                ClientName          = s.Client.Name,
                ProgramName         = s.Program.Name,
                StartDate           = s.StartDate,
                EndDate             = s.EndDate,
                DurationInMonths    = s.DurationInMonths
            })
            .ToListAsync();
    }

    public async Task<SubscriptionDetailsDto?> GetSubscriptionByIdAsync(Guid trainerId, Guid subscriptionId)
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

    public async Task<SubscriptionDetailsDto?> GetClientSubscriptionByIdAsync(Guid clientId, Guid subscriptionId)
    {
        return await _dbContext.Subscriptions
            .Include(s => s.Client)
            .Include(s => s.Program)
            .Where(s => s.ClientId == clientId && s.Id == subscriptionId)
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
		DateTime date = asOfUtc.Date;

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

    public async Task<SubscriptionDetailsDto?> UpdateSubscriptionAsync(Guid subscriptionId, SubscriptionUpdateModel updateModel, Guid trainerId)
    {
		Subscription? subscription = await _dbContext.Subscriptions
            .Include(s => s.Program)
            .FirstOrDefaultAsync(s => s.TrainerId == trainerId && s.Id == subscriptionId);

        if (subscription == null)
            return null;

        if (updateModel.StartDate.HasValue)
        {
            // If the subscription has already started, we cannot change the start date
            if (subscription.StartDate.Date < DateTime.UtcNow.Date)
                throw new InvalidOperationException("Cannot change start date of an already started subscription.");
            
            // If the new start date is in the past, we cannot set it
            if (updateModel.StartDate.Value.Date < DateTime.UtcNow.Date)
                throw new InvalidOperationException("Start date cannot be in the past.");

            subscription.StartDate = updateModel.StartDate.Value;

            await _dbContext.SaveChangesAsync();
        }

        return new SubscriptionDetailsDto
        {
            Id                  = subscription.Id,
            ClientName          = subscription.Client.Name,
            Program             = new ProgramDto
                                    {
                                        Id = subscription.Program.Id,
                                        Name = subscription.Program.Name,
                                        Description = subscription.Program.Description,
                                        UnitPriceExcludingVat = subscription.Program.UnitPriceExcludingVat,
                                        UnitPriceIncludingVat = subscription.Program.UnitPriceIncludingVat,
                                        VatPercentage = subscription.Program.VatPercentage,
                                    },
            StartDate           = subscription.StartDate,
            EndDate             = subscription.EndDate,
            DurationInMonths    = subscription.DurationInMonths
        };     
    }

    public async Task DeactivateSubscriptionAsync(Guid subscriptionId)
    {
		Subscription? subscription = await _dbContext.Subscriptions
            .FirstOrDefaultAsync(s => s.Id == subscriptionId);

        if (subscription != null)
        {
            if (!subscription.IsActive) throw new InvalidOperationException("Subscription is already inactive.");
            subscription.IsActive = false;
            await _dbContext.SaveChangesAsync();
        }
    }
}
