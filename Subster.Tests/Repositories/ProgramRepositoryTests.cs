using Microsoft.EntityFrameworkCore;
using Subster.DAL;
using Subster.DAL.Implementations;
using Subster.Models.InputModels;

namespace Subster.Tests.Repositories;

[TestClass]
public class ProgramRepositoryTests
{
    private SubsterDbContext _ctx = null!;
    private ProgramRepository _repo = null!;

    [TestInitialize]
    public void Init()
    {
		DbContextOptions<SubsterDbContext> opts = new DbContextOptionsBuilder<SubsterDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _ctx = new SubsterDbContext(opts);
        _repo = new ProgramRepository(_ctx);
    }

    [TestMethod]
    public async Task CreateProgramAsync_NegativeVat_ThrowsArgumentException()
    {
        var input = new ProgramInputModel {
            Name = "X",
            VatPercentage = -5,
            UnitPriceExcludingVat = 10m
        };
        await Assert.ThrowsExceptionAsync<ArgumentException>(() =>
            _repo.CreateProgramAsync(input, Guid.NewGuid())
        );
    }

    [TestMethod]
    public async Task CreateProgramAsync_VatOver100_ThrowsArgumentException()
    {
        var input = new ProgramInputModel {
            Name = "X",
            VatPercentage = 150,
            UnitPriceExcludingVat = 10m
        };
        await Assert.ThrowsExceptionAsync<ArgumentException>(() =>
            _repo.CreateProgramAsync(input, Guid.NewGuid())
        );
    }

    [TestMethod]
    public async Task CreateProgramAsync_NoPricesProvided_ThrowsArgumentException()
    {
        var input = new ProgramInputModel {
            Name = "X",
            VatPercentage = 20,
            UnitPriceExcludingVat = null,
            UnitPriceIncludingVat = null
        };
        await Assert.ThrowsExceptionAsync<ArgumentException>(() =>
            _repo.CreateProgramAsync(input, Guid.NewGuid())
        );
    }

    [TestMethod]
    public async Task CreateProgramAsync_NegativePrice_ThrowsArgumentException()
    {
        var input = new ProgramInputModel {
            Name = "X",
            VatPercentage = 20,
            UnitPriceExcludingVat = -1m
        };
        await Assert.ThrowsExceptionAsync<ArgumentException>(() =>
            _repo.CreateProgramAsync(input, Guid.NewGuid())
        );
    }

    [TestMethod]
    public async Task CreateProgramAsync_OnlyExcl_UsesExclAndComputesIncl()
    {
        var input = new ProgramInputModel {
            Name = "X",
            VatPercentage = 25,
            UnitPriceExcludingVat = 100m,
            UnitPriceIncludingVat = 999m, // ignored
            Description = "desc",
            IsActive = true
        };
		Models.Dtos.ProgramDto dto = await _repo.CreateProgramAsync(input, Guid.NewGuid());

        Assert.AreEqual(100m, dto.UnitPriceExcludingVat);
        Assert.AreEqual(125.00m, dto.UnitPriceIncludingVat);
        Assert.AreEqual(input.Name, dto.Name);
        Assert.AreEqual(input.Description, dto.Description);
    }

    [TestMethod]
    public async Task CreateProgramAsync_OnlyIncl_ComputesExclAndUsesIncl()
    {
        var input = new ProgramInputModel {
            Name = "Y",
            VatPercentage = 20,
            UnitPriceExcludingVat = null,
            UnitPriceIncludingVat = 120m,
            Description = "desc2",
            IsActive = false
        };
		Models.Dtos.ProgramDto dto = await _repo.CreateProgramAsync(input, Guid.NewGuid());

        Assert.AreEqual(100.00m, dto.UnitPriceExcludingVat);
        Assert.AreEqual(120m,    dto.UnitPriceIncludingVat);
        Assert.AreEqual(input.Name, dto.Name);
        Assert.AreEqual(input.Description, dto.Description);
    }
}
