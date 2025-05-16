using Subster.DAL.Entities;
using Subster.Models.Dtos;
using Subster.Models.InputModels;
using Subster.Models.UpdateModels;

namespace Subster.API.Services.Interfaces;

public interface ITrainerService
{
    Task<IEnumerable<TrainerDto>> GetAllTrainersAsync();

    Task<TrainerDto?> GetTrainerByIdAsync(Guid trainerId);
    Task<TrainerDetailsDto> GetTrainerDetailsBySsnAsync(string trainerSsn);
    Task<TrainerDetailsDto> UpdateTrainerDetailsAsync(string trainerSsn, TrainerUpdateModel trainerDetailsDto);
    Task<bool> ExistsBySsnAsync(string ssn);
    Task<bool> CreateTrainerIfNotExistsAsync(UserInputModel inputModel);
    Task<Trainer?> GetTrainerEntityBySsnAsync(string ssn);
    Task<bool> HasPaydayCredentialsAsync(string ssn);
    Task<bool> UpdatePaydayCredentialsAsync(string ssn, string clientId, string clientSecret);

    /// <summary>
    /// Deletes the trainer's Payday credentials.
    /// Returns false if trainer not found.
    /// </summary>
    Task<bool> DeletePaydayCredentialsAsync(string ssn);

    /// <summary>
    /// Deletes (or soft-deletes) a trainer by SSN.
    /// Returns false if trainer not found.
    /// </summary>
    Task<bool> DeleteTrainerAsync(string ssn);
}