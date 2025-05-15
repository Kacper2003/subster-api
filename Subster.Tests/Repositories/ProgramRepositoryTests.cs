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
        var opts = new DbContextOptionsBuilder<SubsterDbContext>()
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
        var dto = await _repo.CreateProgramAsync(input, Guid.NewGuid());

        // excl must be 100, incl = round(100 * 1.25, 2) = 125.00
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
        var dto = await _repo.CreateProgramAsync(input, Guid.NewGuid());

        // incl = 120, excl = round(120 / 1.2, 2) = 100.00
        Assert.AreEqual(100.00m, dto.UnitPriceExcludingVat);
        Assert.AreEqual(120m,    dto.UnitPriceIncludingVat);
        Assert.AreEqual(input.Name, dto.Name);
        Assert.AreEqual(input.Description, dto.Description);
    }
}
