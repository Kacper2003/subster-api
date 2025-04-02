using Subster.Models.Dtos;
using Subster.Models.InputModels;

namespace Subster.API.Services.Interfaces;

public interface ITrainerService
{
    Task<IEnumerable<TrainerDto>> GetAllTrainersAsync();
    Task<bool> CreateTrainerIfNotExistsAsync(UserInputModel inputModel);
}