using Subster.API.Exceptions;
using Subster.DAL.Interfaces;
using Subster.API.Services.Interfaces;
using Subster.Models.InputModels;
using Subster.Models.Dtos;
using Subster.Models.UpdateModels;

namespace Subster.API.Services.Implementations;

public class ProgramService : IProgramService
{
    private readonly ITrainerRepository _trainerRepository;
    private readonly IProgramRepository _programRepository;

    public ProgramService(
        ITrainerRepository trainerRepository,
        IProgramRepository programRepository)
    {
        _trainerRepository  = trainerRepository;
        _programRepository  = programRepository;
    }

    public async Task<ProgramDto> CreateProgramAsync(
        ProgramInputModel inputModel,
        string trainerSsn)
    {
        var trainer = await _trainerRepository.GetTrainerBySsnAsync(trainerSsn)
            ?? throw new UnauthorizedException("Invalid trainer credentials.");

        return await _programRepository.CreateProgramAsync(inputModel, trainer.Id);
    }

    public async Task<ProgramDto> UpdateProgramAsync(
        Guid programId,
        ProgramUpdateModel updateModel,
        string trainerSsn)
    {
        var trainer = await _trainerRepository.GetTrainerBySsnAsync(trainerSsn) 
            ?? throw new UnauthorizedException("Invalid trainer credentials.");

        // Will return null if not found or not owned → we map to 404
        var updatedProgram = await _programRepository.UpdateProgramAsync(programId, updateModel, trainer.Id)
            ?? throw new NotFoundException($"Program with id {programId} not found.");

        return updatedProgram;
    }

    public async Task<IEnumerable<ProgramDto>> GetAllProgramsAsync(string trainerSsn)
    {
        var trainer = await _trainerRepository.GetTrainerBySsnAsync(trainerSsn)
            ?? throw new UnauthorizedException("Invalid trainer credentials.");

        return await _programRepository.GetAllProgramsAsync(trainer.Id);
    }
    
    public async Task<ProgramDto> GetProgramByIdAsync(
        string trainerSsn,
        Guid programId)
    {
        var trainer = await _trainerRepository.GetTrainerBySsnAsync(trainerSsn)
            ?? throw new UnauthorizedException("Invalid trainer credentials.");

        var program = await _programRepository.GetProgramByIdAsync(trainer.Id, programId)
            ?? throw new NotFoundException($"Program with id {programId} not found.");

        return program;
    }

    public async Task DeactivateProgramAsync(string trainerSsn, Guid programId)
    {
        var trainer = await _trainerRepository.GetTrainerBySsnAsync(trainerSsn)
            ?? throw new UnauthorizedException("Invalid trainer credentials.");

        // Because Deactivate returns void, we check existence first:
        var existing = await _programRepository.GetProgramByIdAsync(trainer.Id, programId)
            ?? throw new NotFoundException($"Program with id {programId} not found.");

        await _programRepository.DeactivateProgramAsync(trainer.Id, programId);
    }
}
