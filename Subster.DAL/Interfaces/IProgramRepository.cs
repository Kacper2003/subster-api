using Subster.Models.Dtos;
using Subster.Models.InputModels;
using Subster.Models.UpdateModels;

namespace Subster.DAL.Interfaces;

public interface IProgramRepository
{
    Task<ProgramDto> CreateProgramAsync(ProgramInputModel inputModel, Guid trainerId);
    Task<IEnumerable<ProgramDto>> GetAllProgramsAsync(Guid trainerId);
    Task<ProgramDto?> GetProgramByIdAsync(Guid trainerId, Guid programId);
    Task<ProgramDto?> UpdateProgramAsync(Guid programId, ProgramUpdateModel updateModel, Guid trainerId);
    Task DeactivateProgramAsync(Guid trainerId, Guid programId);
}