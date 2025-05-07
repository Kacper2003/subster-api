using Subster.Models.Dtos;
using Subster.Models.InputModels;
using Subster.Models.UpdateModels;

namespace Subster.API.Services.Interfaces;

public interface IProgramService
{
    Task<ProgramDto> CreateProgramAsync(ProgramInputModel inputModel, string trainerSsn);
    Task<IEnumerable<ProgramDto>> GetAllProgramsAsync(string trainerSsn);
    Task<ProgramDto> GetProgramByIdAsync(string trainerSsn, Guid programId);
    Task<ProgramDto> UpdateProgramAsync(Guid programId, ProgramUpdateModel updateModel, string trainerSsn);
    Task DeactivateProgramAsync(string trainerSsn, Guid programId);
}