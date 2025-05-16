using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Subster.DAL.Interfaces;
using Subster.DAL.Entities;
using Subster.DAL.Utilities;
using Subster.Models.Dtos;
using Subster.Models.InputModels;
using Subster.Models.UpdateModels;

namespace Subster.DAL.Implementations
{
    public class TrainerRepository(SubsterDbContext dbContext, EncryptionHelper encryptionHelper) : ITrainerRepository
    {
        private readonly SubsterDbContext _dbContext = dbContext;
        private readonly EncryptionHelper _encryptionHelper = encryptionHelper;

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

        public async Task<TrainerDto?> GetTrainerByIdAsync(Guid trainerId)
        {
            return await _dbContext.Trainers
                .Where(u => u.Id == trainerId)
                .Select(u => new TrainerDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Ssn = u.Ssn,
                    PhoneNumber = u.PhoneNumber
                })
                .FirstOrDefaultAsync();
        }

        public async Task<TrainerDetailsDto?> GetTrainerDetailsByIdAsync(Guid trainerId)
        {
			TrainerDetailsDto? trainer = await _dbContext.Trainers
                .Where(u => u.Id == trainerId)
                .Select(u => new TrainerDetailsDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Ssn = u.Ssn,
                    PhoneNumber = u.PhoneNumber,
                    Programs = u.Programs.Select(p => new ProgramDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        UnitPriceExcludingVat = p.UnitPriceExcludingVat,
                        UnitPriceIncludingVat = p.UnitPriceIncludingVat,
                        VatPercentage = p.VatPercentage,
                        IsActive = p.IsActive
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            return trainer;
        }

        public async Task<TrainerDetailsDto?> UpdateTrainerDetailsAsync(Guid trainerId, TrainerUpdateModel updateModel)
        {
			Trainer? trainer = await _dbContext.Trainers
                .Include(u => u.Programs)
                .FirstOrDefaultAsync(u => u.Id == trainerId);

            if (trainer == null)
                return null;

            if (!string.IsNullOrWhiteSpace(updateModel.Name))
                trainer.Name = updateModel.Name;

            if (!string.IsNullOrWhiteSpace(updateModel.PhoneNumber))
                trainer.PhoneNumber = updateModel.PhoneNumber;

            await _dbContext.SaveChangesAsync();

            return new TrainerDetailsDto
            {
                Id = trainer.Id,
                Name = trainer.Name,
                Ssn = trainer.Ssn,
                PhoneNumber = trainer.PhoneNumber,
                Programs = trainer.Programs.Select(p => new ProgramDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    UnitPriceExcludingVat = p.UnitPriceExcludingVat,
                    UnitPriceIncludingVat = p.UnitPriceIncludingVat,
                    VatPercentage = p.VatPercentage,
                    IsActive = p.IsActive
                }).ToList()
            };
        }

        public async Task<bool> ExistsBySsnAsync(string ssn)
        {
            return await _dbContext.Trainers
                .AsNoTracking()
                .AnyAsync(u => u.Ssn == ssn);
        }

        public async Task<Trainer?> FindTrainerEntityBySsnAsync(string ssn)
        {
			Trainer? trainer = await _dbContext.Trainers
                .FirstOrDefaultAsync(u => u.Ssn == ssn);

            if (trainer != null)
            {
                trainer.PaydayClientId = trainer.PaydayClientId == null
                    ? null
                    : _encryptionHelper.Unprotect(trainer.PaydayClientId);
                trainer.PaydayClientSecret = trainer.PaydayClientSecret == null
                    ? null
                    : _encryptionHelper.Unprotect(trainer.PaydayClientSecret);
            }

            return trainer;
        }

        public async Task<Trainer?> FindTrainerEntityByIdAsync(Guid trainerId)
        {
			Trainer? trainer = await _dbContext.Trainers
                .FirstOrDefaultAsync(u => u.Id == trainerId);

            if (trainer != null)
            {
                trainer.PaydayClientId = trainer.PaydayClientId == null
                    ? null
                    : _encryptionHelper.Unprotect(trainer.PaydayClientId);
                trainer.PaydayClientSecret = trainer.PaydayClientSecret == null
                    ? null
                    : _encryptionHelper.Unprotect(trainer.PaydayClientSecret);
            }

            return trainer;
        }

        public async Task CreateTrainerAsync(UserInputModel inputModel)
        {
			// Use async lookup
			Trainer? existingTrainer = await _dbContext.Trainers
                .FirstOrDefaultAsync(u => u.Ssn == inputModel.Ssn);

            if (existingTrainer == null)
            {
                var trainer = new Trainer
                {
                    Name = inputModel.Name,
                    Ssn = inputModel.Ssn,
                    PhoneNumber = inputModel.PhoneNumber,
                };

                _dbContext.Trainers.Add(trainer);
            }
            else
            {
                existingTrainer.Name = inputModel.Name;
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdatePaydayCredentialsAsync(Guid trainerId, string? clientId, string? clientSecret)
        {
			Trainer? trainer = await _dbContext.Trainers
                .FirstOrDefaultAsync(u => u.Id == trainerId);

            if (trainer == null)
                return;

            trainer.PaydayClientId = clientId == null
                ? null
                : _encryptionHelper.Protect(clientId);

            trainer.PaydayClientSecret = clientSecret == null
                ? null
                : _encryptionHelper.Protect(clientSecret);

            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteTrainerAsync(Guid trainerId)
        {
			Trainer? trainer = await _dbContext.Trainers
                .FirstOrDefaultAsync(u => u.Id == trainerId);

            if (trainer == null)
                return;

            _dbContext.Trainers.Remove(trainer);
            await _dbContext.SaveChangesAsync();
        }
    }
}
