using Subster.DAL.Entities;
using Subster.Models.Dtos;
using Subster.Models.InputModels;

namespace Subster.API.Services.Interfaces;

public interface ITrainerService
{
    /// <summary>
    /// Retrieves all trainers' public DTOs.
    /// </summary>
    Task<IEnumerable<TrainerDto>> GetAllTrainersAsync();

    /// <summary>
    /// Retrieves a public DTO for a trainer by internal ID.
    /// </summary>
    Task<TrainerDto?> GetTrainerByIdAsync(int id);

    /// <summary>
    /// Checks whether a trainer with the given SSN exists.
    /// </summary>
    Task<bool> ExistsBySsnAsync(string ssn);

    /// <summary>
    /// Creates a trainer if none exists with the same SSN.
    /// Returns true if created, false otherwise.
    /// </summary>
    Task<bool> CreateTrainerIfNotExistsAsync(UserInputModel inputModel);

    /// <summary>
    /// Retrieves the full Trainer entity (including decrypted credentials) by SSN.
    /// </summary>
    Task<Trainer?> GetTrainerEntityBySsnAsync(string ssn);

    /// <summary>
    /// Updates the trainer's Payday credentials.
    /// Returns false if trainer not found.
    /// </summary>
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