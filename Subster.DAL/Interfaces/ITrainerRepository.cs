using Subster.Models.Dtos;
using Subster.Models.InputModels;
using Subster.DAL.Entities;

namespace Subster.DAL.Interfaces;

public interface ITrainerRepository
{
    Task<IEnumerable<TrainerDto>> GetAllTrainersAsync();
    Task<TrainerDto?> GetTrainerBySsnAsync(string ssn);
    Task<Trainer?> GetTrainerEntityBySsnAsync(string ssn);
    Task CreateTrainerAsync(UserInputModel inputModel);
    Task UpdatePaydayCredentialsAsync(int trainerId, string? clientId, string? clientSecret);
}