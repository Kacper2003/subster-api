using Subster.DAL.Interfaces;
using Subster.DAL.Entities;
using Subster.Models.InputModels;
using Subster.Models.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Subster.DAL.Implementations;

public class ProgramRepository : IProgramRepository
{
    private readonly SubsterDbContext _dbContext;

    public ProgramRepository(SubsterDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task CreateProgramAsync(ProgramInputModel inputModel, int trainerId)
    {
        // Validate the vat
        if (inputModel.VatPercentage < 0 || inputModel.VatPercentage > 100)
            throw new ArgumentException("VatPercentage must be between 0 and 100.");

        // Check for values
        var exclProvided = inputModel.UnitPriceExcludingVat.HasValue;
        var inclProvided = inputModel.UnitPriceIncludingVat.HasValue;

        // Neither provided → error
        if (!exclProvided && !inclProvided)
            throw new ArgumentException("Either UnitPriceExcludingVat or UnitPriceIncludingVat must be provided.");

        // If both provided, Payday says to ignore the including-VAT value
        // so we only use excluding-VAT in that case:
        if (exclProvided && inputModel.UnitPriceExcludingVat < 0
        || inclProvided && inputModel.UnitPriceIncludingVat < 0)
        {
            throw new ArgumentException("Price values cannot be negative.");
        }

        // Compute both excl and incl
        decimal excl, incl;
        if (exclProvided)
        {
            excl = inputModel.UnitPriceExcludingVat!.Value;
            incl = Math.Round(excl * (1 + inputModel.VatPercentage / 100m), 2);
        }
        else
        {
            // Only including-VAT provided (excl is null)
            incl = inputModel.UnitPriceIncludingVat!.Value;
            excl = Math.Round(incl / (1 + inputModel.VatPercentage / 100m), 2);
        }

        var program = new Program
        {
            Name                   = inputModel.Name,
            Description            = inputModel.Description,
            UnitPriceExcludingVat  = excl,
            UnitPriceIncludingVat  = incl,
            VatPercentage          = inputModel.VatPercentage,
            IsActive               = inputModel.IsActive,
            TrainerId              = trainerId,
        };

        await _dbContext.Programs.AddAsync(program);
        await _dbContext.SaveChangesAsync();
    }


    public async Task<IEnumerable<ProgramDto>> GetAllProgramsAsync(int trainerId)
    {
        // only active programs
        var programs = await _dbContext.Programs
            .Where(p => p.TrainerId == trainerId && p.IsActive)
            .Select(p => new ProgramDto
            {
                Id                      = p.Id,
                Name                    = p.Name,
                Description             = p.Description,
                UnitPriceExcludingVat   = p.UnitPriceExcludingVat,
                UnitPriceIncludingVat   = p.UnitPriceIncludingVat,
                VatPercentage           = p.VatPercentage,
            })
            .ToListAsync();

        return programs;
    }

    public async Task<ProgramDto?> GetProgramByIdAsync(int trainerId, int programId)
    {
        var program = await _dbContext.Programs
            .Where(p => p.TrainerId == trainerId && p.Id == programId && p.IsActive)
            .Select(p => new ProgramDto
            {
                Id                      = p.Id,
                Name                    = p.Name,
                Description             = p.Description,
                UnitPriceExcludingVat   = p.UnitPriceExcludingVat,
                UnitPriceIncludingVat   = p.UnitPriceIncludingVat,
                VatPercentage           = p.VatPercentage,
            })
            .FirstOrDefaultAsync();

        return program;
    }
    
    public async Task DeactivateProgramAsync(int trainerId, int programId)
    {
        var program = await _dbContext.Programs
            .FirstOrDefaultAsync(p => p.TrainerId == trainerId && p.Id == programId);

        if (program != null)
        {
            program.IsActive = false;
            await _dbContext.SaveChangesAsync();
        }
    }
}