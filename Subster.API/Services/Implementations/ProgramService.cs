using Subster.DAL.Interfaces;
using Subster.API.Services.Interfaces;
using Subster.Models.InputModels;
using Subster.Models.Dtos;

namespace Subster.API.Services.Implementations;

public class ProgramService : IProgramService
{
    private readonly ITrainerRepository _trainerRepository;
    private readonly IProgramRepository _programRepository;

    public ProgramService(ITrainerRepository trainerRepository, IProgramRepository programRepository)
    {
        _trainerRepository = trainerRepository;
        _programRepository = programRepository;
    }

    public async Task CreateProgramAsync(ProgramInputModel inputModel, string trainerSsn)
    {
        var trainer = await _trainerRepository.GetTrainerBySsnAsync(trainerSsn);
        if (trainer == null)
        {
            throw new Exception("Trainer not found");
        }

        await _programRepository.CreateProgramAsync(inputModel, trainer.Id);
    }

    public async Task<IEnumerable<ProgramDto>> GetAllProgramsAsync(string trainerSsn)
    {
        var trainer = await _trainerRepository.GetTrainerBySsnAsync(trainerSsn);
        if (trainer == null)
        {
            throw new Exception("Trainer not found");
        }

        return await _programRepository.GetAllProgramsAsync(trainer.Id);
    }
    
    public async Task<ProgramDto> GetProgramByIdAsync(string trainerSsn, int programId)
    {
        var trainer = await _trainerRepository.GetTrainerBySsnAsync(trainerSsn);
        if (trainer == null)
        {
            throw new Exception("Trainer not found");
        }

        var program = await _programRepository.GetProgramByIdAsync(trainer.Id, programId);
        if (program == null)
        {
            throw new Exception("Program not found");
        }

        return program;
    }

    public async Task DeactivateProgramAsync(string trainerSsn, int programId)
    {
        var trainer = await _trainerRepository.GetTrainerBySsnAsync(trainerSsn);
        if (trainer == null)
        {
            throw new Exception("Trainer not found");
        }

        await _programRepository.DeactivateProgramAsync(trainer.Id, programId);
    }
}