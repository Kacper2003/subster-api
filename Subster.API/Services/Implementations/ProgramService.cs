using Subster.API.Exceptions;
using Subster.DAL.Interfaces;
using Subster.API.Services.Interfaces;
using Subster.Models.InputModels;
using Subster.Models.Dtos;
using Subster.Models.UpdateModels;

namespace Subster.API.Services.Implementations;

public class ProgramService(ITrainerRepository trainerRepository, IProgramRepository programRepository) : IProgramService
{
    private readonly ITrainerRepository _trainerRepository = trainerRepository;
    private readonly IProgramRepository _programRepository = programRepository;

	public async Task<ProgramDto> CreateProgramAsync(ProgramInputModel inputModel, string trainerSsn)
    {
		TrainerDto trainer = await _trainerRepository.GetTrainerBySsnAsync(trainerSsn)
            ?? throw new UnauthorizedException("Invalid trainer credentials.");

        return await _programRepository.CreateProgramAsync(inputModel, trainer.Id);
    }

    public async Task<ProgramDto> UpdateProgramAsync(Guid programId, ProgramUpdateModel updateModel, string trainerSsn)
    {
		TrainerDto trainer = await _trainerRepository.GetTrainerBySsnAsync(trainerSsn) 
            ?? throw new UnauthorizedException("Invalid trainer credentials.");

		ProgramDto updatedProgram = await _programRepository.UpdateProgramAsync(programId, updateModel, trainer.Id)
            ?? throw new NotFoundException($"Program with id {programId} not found.");

        return updatedProgram;
    }

    public async Task<IEnumerable<ProgramDto>> GetAllProgramsAsync(string trainerSsn)
    {
		TrainerDto trainer = await _trainerRepository.GetTrainerBySsnAsync(trainerSsn)
            ?? throw new UnauthorizedException("Invalid trainer credentials.");

        return await _programRepository.GetAllProgramsAsync(trainer.Id);
    }
    
    public async Task<ProgramDto> GetProgramByIdAsync(string trainerSsn, Guid programId)
    {
		TrainerDto trainer = await _trainerRepository.GetTrainerBySsnAsync(trainerSsn)
            ?? throw new UnauthorizedException("Invalid trainer credentials.");

		ProgramDto program = await _programRepository.GetProgramByIdAsync(trainer.Id, programId)
            ?? throw new NotFoundException($"Program with id {programId} not found.");

        return program;
    }

    public async Task DeactivateProgramAsync(string trainerSsn, Guid programId)
    {
		TrainerDto trainer = await _trainerRepository.GetTrainerBySsnAsync(trainerSsn)
            ?? throw new UnauthorizedException("Invalid trainer credentials.");

		// Because Deactivate returns void, we check existence first
		ProgramDto existing = await _programRepository.GetProgramByIdAsync(trainer.Id, programId)
            ?? throw new NotFoundException($"Program with id {programId} not found.");

        await _programRepository.DeactivateProgramAsync(trainer.Id, programId);
    }
}
