using Subster.Models.Dtos;
using Subster.Models.InputModels;
using Subster.Models.UpdateModels;

namespace Subster.DAL.Interfaces;

public interface IProgramRepository
{
    Task<ProgramDto> CreateProgramAsync(ProgramInputModel inputModel, int trainerId);
    Task<IEnumerable<ProgramDto>> GetAllProgramsAsync(int trainerId);
    Task<ProgramDto?> GetProgramByIdAsync(int trainerId, Guid programId);
    Task<ProgramDto?> UpdateProgramAsync(Guid programId, ProgramUpdateModel updateModel, int trainerId);
    Task DeactivateProgramAsync(int trainerId, Guid programId);
}