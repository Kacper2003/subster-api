using Subster.API.Services.Interfaces;
using Subster.DAL.Interfaces;
using Subster.Models.Dtos;
using Subster.Models.InputModels;

namespace Subster.API.Services.Implementations;

public class TrainerService : ITrainerService
{
    private readonly ITrainerRepository _trainerRepository;

    public TrainerService(ITrainerRepository trainerRepository)
    {
        _trainerRepository = trainerRepository;
    }

    public async Task<IEnumerable<TrainerDto>> GetAllTrainersAsync()
    {
        return await _trainerRepository.GetAllTrainersAsync();
    }

    public async Task<bool> CreateTrainerIfNotExistsAsync(UserInputModel inputModel)
    {
        var trainer = await _trainerRepository.GetTrainerBySsnAsync(inputModel.Ssn);
        if (trainer == null)
        {
            await _trainerRepository.CreateTrainerAsync(inputModel);
            return true;
        }
        return false;
    }
}