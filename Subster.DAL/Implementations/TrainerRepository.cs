using Microsoft.EntityFrameworkCore;
using Subster.DAL.Interfaces;
using Subster.DAL.Entities;
using Subster.DAL.Utilities;
using Subster.Models.Dtos;
using Subster.Models.InputModels;


namespace Subster.DAL.Implementations;

public class TrainerRepository : ITrainerRepository
{
    private readonly SubsterDbContext _dbContext;
    private readonly EncryptionHelper _encryptionHelper;

    public TrainerRepository(SubsterDbContext dbContext, EncryptionHelper encryptionHelper)
    {
        _dbContext = dbContext;
        _encryptionHelper = encryptionHelper;
    }

    public async Task<IEnumerable<TrainerDto>> GetAllTrainersAsync()
    {
        return await _dbContext.Trainers
            .Select(u => new TrainerDto
            {
                Id = u.Id,
                Name = u.Name,
                Ssn = u.Ssn,
                PhoneNumber = u.PhoneNumber
            })
            .ToListAsync();
    }

    public async Task<TrainerDto?> GetTrainerBySsnAsync(string ssn)
    {
        return await _dbContext.Trainers
            .Where(u => u.Ssn == ssn)
            .Select(u => new TrainerDto
            {
                Id = u.Id,
                Name = u.Name,
                Ssn = u.Ssn,
                PhoneNumber = u.PhoneNumber
            })
            .FirstOrDefaultAsync();
    }

    public async Task<Trainer?> GetTrainerEntityBySsnAsync(string ssn)
    {
        var trainer = await _dbContext.Trainers
            .FirstOrDefaultAsync(u => u.Ssn == ssn);

        if (trainer != null)
        {
            // Decrypt if not null
            trainer.PaydayClientId = (trainer.PaydayClientId == null) ? null : _encryptionHelper.Unprotect(trainer.PaydayClientId);
            trainer.PaydayClientSecret = (trainer.PaydayClientSecret == null) ? null : _encryptionHelper.Unprotect(trainer.PaydayClientSecret);
        }

        return trainer;
    }

    public async Task CreateTrainerAsync(TrainerInputModel inputModel)
    {
        // Check if trainer already exists
        var existingTrainer = _dbContext.Trainers
            .FirstOrDefault(u => u.Ssn == inputModel.Ssn);

        if (existingTrainer == null)
        {
            var trainer = new Trainer
            {
                Name = inputModel.Name,
                Ssn = inputModel.Ssn,
                PhoneNumber = "123-4567",
                CreatedAt = DateTime.UtcNow
            };

            // Save new trainer to database
            _dbContext.Trainers.Add(trainer);
            await _dbContext.SaveChangesAsync();
        } else {
            // Update trainer name if it has changed
            existingTrainer.Name = inputModel.Name;
            await _dbContext.SaveChangesAsync();
        }
    }
    
    public async Task UpdatePaydayCredentialsAsync(int trainerId, string? clientId, string? clientSecret)
    {
        var trainer = await _dbContext.Trainers
            .FirstOrDefaultAsync(u => u.Id == trainerId);

        if (trainer != null)
        {
            // Encrypt if updating and not deleting
            trainer.PaydayClientId = (clientId == null) ? null : _encryptionHelper.Protect(clientId);
            trainer.PaydayClientSecret = (clientSecret == null) ? null : _encryptionHelper.Protect(clientSecret);
            await _dbContext.SaveChangesAsync();
        }
    }
}
