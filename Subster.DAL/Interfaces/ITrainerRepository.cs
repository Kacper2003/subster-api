using Subster.Models.Dtos;
using Subster.Models.InputModels;
using Subster.DAL.Entities;

namespace Subster.DAL.Interfaces;

public interface ITrainerRepository
{
    // existing:
    Task<IEnumerable<TrainerDto>> GetAllTrainersAsync();
    Task<TrainerDto?>           GetTrainerBySsnAsync(string ssn);
    Task<Trainer?>              FindTrainerEntityBySsnAsync(string ssn);
    Task                        CreateTrainerAsync(UserInputModel input);
    Task                        UpdatePaydayCredentialsAsync(int trainerId, string? clientId, string? clientSecret);
    Task<Trainer?> FindTrainerEntityByIdAsync(int id);
    Task<TrainerDto?> GetTrainerByIdAsync(int id);
    Task DeleteTrainerAsync(int id);
    Task<bool> ExistsBySsnAsync(string ssn);
}
