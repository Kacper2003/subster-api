using Subster.Models.Dtos;
using Subster.Models.InputModels;

namespace Subster.DAL.Interfaces;

public interface IProgramRepository
{
    Task CreateProgramAsync(ProgramInputModel inputModel, int trainerId);
    Task<IEnumerable<ProgramDto>> GetAllProgramsAsync(int trainerId);
    Task<ProgramDto?> GetProgramByIdAsync(int trainerId, int programId);
    Task DeactivateProgramAsync(int trainerId, int programId);
}