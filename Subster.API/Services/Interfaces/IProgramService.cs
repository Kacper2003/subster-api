using Subster.Models.Dtos;
using Subster.Models.InputModels;

namespace Subster.API.Services.Interfaces;

public interface IProgramService
{
    Task CreateProgramAsync(ProgramInputModel inputModel, string trainerSsn);
    Task<IEnumerable<ProgramDto>> GetAllProgramsAsync(string trainerSsn);
    Task<ProgramDto> GetProgramByIdAsync(string trainerSsn, int programId);
    Task DeactivateProgramAsync(string trainerSsn, int programId);
}