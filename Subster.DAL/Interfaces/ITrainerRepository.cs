using Subster.Models.Dtos;
using Subster.Models.InputModels;
using Subster.Models.UpdateModels;
using Subster.DAL.Entities;

namespace Subster.DAL.Interfaces;

public interface ITrainerRepository
{
    // existing:
    Task<IEnumerable<TrainerDto>> GetAllTrainersAsync();
    Task<TrainerDto?>           GetTrainerBySsnAsync(string ssn);
    Task<Trainer?>              FindTrainerEntityBySsnAsync(string ssn);
    Task<TrainerDetailsDto?> GetTrainerDetailsByIdAsync(Guid trainerId);
    Task<TrainerDetailsDto?> UpdateTrainerDetailsAsync(Guid trainerId, TrainerUpdateModel updateModel);
    Task                        CreateTrainerAsync(UserInputModel input);
    Task                        UpdatePaydayCredentialsAsync(Guid trainerId, string? clientId, string? clientSecret);
    Task<Trainer?> FindTrainerEntityByIdAsync(Guid trainerId);
    Task<TrainerDto?> GetTrainerByIdAsync(Guid trainerId);
    Task DeleteTrainerAsync(Guid trainerId);
    Task<bool> ExistsBySsnAsync(string ssn);
}
