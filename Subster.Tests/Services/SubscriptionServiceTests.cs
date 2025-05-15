using Moq;
using Microsoft.Extensions.Configuration;
using Subster.API.Services.Implementations;
using Subster.API.Services.Interfaces;
using Subster.DAL.Interfaces;
using Subster.Models.InputModels;
using Subster.Models.Dtos;
using Subster.API.Exceptions;
using Subster.Models.ResponseModels;

namespace Subster.Tests.Services;

[TestClass]
public class SubscriptionServiceTests
{
    private Mock<IPaydayService>          _mockPaydayService = null!;
    private Mock<ITaktikalAuthService>    _mockTaktikalAuthService = null!;
    private Mock<IClientRepository>       _mockClientRepository = null!;
    private Mock<ITrainerRepository>      _mockTrainerRepository = null!;
    private Mock<ISubscriptionRepository> _mockSubscriptionRepository = null!;
    private Mock<IProgramRepository>      _mockProgramRepository = null!;
    private SubscriptionService           _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockPaydayService          = new Mock<IPaydayService>();
        _mockTaktikalAuthService    = new Mock<ITaktikalAuthService>();
        _mockClientRepository       = new Mock<IClientRepository>();
        _mockTrainerRepository      = new Mock<ITrainerRepository>();
        _mockSubscriptionRepository = new Mock<ISubscriptionRepository>();
        _mockProgramRepository      = new Mock<IProgramRepository>();

        _service = new SubscriptionService(
            _mockPaydayService.Object,
            _mockTrainerRepository.Object,
            _mockSubscriptionRepository.Object,
            _mockProgramRepository.Object,
            _mockTaktikalAuthService.Object,
            _mockClientRepository.Object
        );
    }

    [TestMethod]
    public async Task CreateSubscriptionAsync_TrainerNotFound_ThrowsUnauthorizedException()
    {
        // Arrange
        _mockTrainerRepository
            .Setup(r => r.GetTrainerBySsnAsync("T-SSN"))
            .ReturnsAsync((TrainerDto?)null);

        var input = new SubscriptionInputModel
        {
            ProgramId = Guid.NewGuid(),
            StartDate = DateTime.UtcNow,
            DurationInMonths = 1
        };

        // Act & Assert
        await Assert.ThrowsExceptionAsync<UnauthorizedException>(() =>
            _service.CreateSubscriptionAsync(input, "T-SSN")
        );
    }

    [TestMethod]
    public async Task CreateSubscriptionAsync_ProgramNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var trainer = new TrainerDto { Id = Guid.NewGuid(), Ssn = "T-SSN", Name = "T", PhoneNumber = "P" };
        _mockTrainerRepository
            .Setup(r => r.GetTrainerBySsnAsync("T-SSN"))
            .ReturnsAsync(trainer);
        _mockProgramRepository
            .Setup(p => p.GetProgramByIdAsync(trainer.Id, It.IsAny<Guid>()))
            .ReturnsAsync((ProgramDto?)null);

        var input = new SubscriptionInputModel
        {
            ProgramId = Guid.NewGuid(),
            StartDate = DateTime.UtcNow,
            DurationInMonths = 1
        };

        // Act & Assert
        await Assert.ThrowsExceptionAsync<NotFoundException>(() =>
            _service.CreateSubscriptionAsync(input, "T-SSN")
        );
    }

    [TestMethod]
    public async Task CreateSubscriptionAsync_ClientAuthFails_ThrowsValidationException()
    {
        // Arrange
        var trainer = new TrainerDto { Id = Guid.NewGuid(), Ssn = "T-SSN", Name = "T", PhoneNumber = "P" };
        var program = new ProgramDto { Id = Guid.NewGuid(), Name = "X", UnitPriceExcludingVat = 10m, UnitPriceIncludingVat = 12m, VatPercentage = 20, IsActive = true };

        _mockTrainerRepository
            .Setup(r => r.GetTrainerBySsnAsync("T-SSN"))
            .ReturnsAsync(trainer);
        _mockProgramRepository
            .Setup(p => p.GetProgramByIdAsync(trainer.Id, program.Id))
            .ReturnsAsync(program);

        _mockTaktikalAuthService
            .Setup(a => a.AuthenticateAsync(It.IsAny<AuthInputModel>()))
            .ReturnsAsync(new EndAuthResponseModel
            {
                Authenticated = false,
                Error         = "bad creds",
                StatusCode    = 400
            });

        var input = new SubscriptionInputModel
        {
            ClientSsn         = "C-SSN",
            StartDate         = DateTime.UtcNow,
            DurationInMonths  = 1,
            ProgramId         = program.Id
        };

        // Act & Assert
        await Assert.ThrowsExceptionAsync<ValidationException>(() =>
            _service.CreateSubscriptionAsync(input, "T-SSN")
        );
    }

    [TestMethod]
    public async Task CreateSubscriptionAsync_HappyPath_ReturnsSubscriptionAndInvokesAllSteps()
    {
        // Arrange
        var trainer   = new TrainerDto { Id = Guid.NewGuid(), Ssn = "T-SSN", Name = "T", PhoneNumber = "P" };
        var program   = new ProgramDto { Id = Guid.NewGuid(), Name = "X", UnitPriceExcludingVat = 10m, UnitPriceIncludingVat = 12m, VatPercentage = 20, IsActive = true };
        var customer  = new CustomerDto { Name = "C", Ssn = "C-SSN" };
        var clientId  = Guid.NewGuid();
        var invoiceId = "INV-123";
        var subscription = new SubscriptionDetailsDto
        {
            Id                 = Guid.NewGuid(),
            ClientName         = "C",
            Program            = program,
            StartDate          = DateTime.UtcNow,
            EndDate            = DateTime.UtcNow.AddMonths(1),
            DurationInMonths   = 1
        };

        _mockTrainerRepository
            .Setup(r => r.GetTrainerBySsnAsync("T-SSN"))
            .ReturnsAsync(trainer);
        _mockProgramRepository
            .Setup(p => p.GetProgramByIdAsync(trainer.Id, program.Id))
            .ReturnsAsync(program);
        _mockTaktikalAuthService
            .Setup(a => a.AuthenticateAsync(It.IsAny<AuthInputModel>()))
            .ReturnsAsync(new EndAuthResponseModel
            {
                Authenticated = true,
                Customer      = customer
            });
        _mockClientRepository
            .Setup(c => c.CreateClientAsync(It.Is<UserInputModel>(u => u.Name == customer.Name && u.Ssn == customer.Ssn)))
            .ReturnsAsync(clientId);
        _mockSubscriptionRepository
            .Setup(s => s.CreateSubscriptionAsync(It.IsAny<SubscriptionInputModel>(), trainer.Id, clientId))
            .ReturnsAsync(subscription);
        _mockPaydayService
            .Setup(p => p.CreateInvoiceAsync("T-SSN", customer.Ssn, program))
            .ReturnsAsync(invoiceId);

        var input = new SubscriptionInputModel
        {
            ClientSsn         = customer.Ssn,
            StartDate         = subscription.StartDate,
            DurationInMonths  = subscription.DurationInMonths,
            ProgramId         = program.Id
        };

        // Act
        var result = await _service.CreateSubscriptionAsync(input, "T-SSN");

        // Assert
        Assert.AreEqual(subscription.Id, result.Id);
        _mockTrainerRepository.Verify(r => r.GetTrainerBySsnAsync("T-SSN"), Times.Once);
        _mockProgramRepository.Verify(p => p.GetProgramByIdAsync(trainer.Id, program.Id), Times.Once);
        _mockTaktikalAuthService.Verify(a => a.AuthenticateAsync(It.IsAny<AuthInputModel>()), Times.Once);
        _mockClientRepository.Verify(c => c.CreateClientAsync(It.IsAny<UserInputModel>()), Times.Once);
        _mockSubscriptionRepository.Verify(s => s.CreateSubscriptionAsync(input, trainer.Id, clientId), Times.Once);
        _mockPaydayService.Verify(p => p.CreateInvoiceAsync("T-SSN", customer.Ssn, program), Times.Once);
        _mockSubscriptionRepository.Verify(s => s.CreateSubscriptionInvoiceAsync(subscription.Id, invoiceId, 1), Times.Once);
    }

    [TestMethod]
    public async Task CreateSubscriptionAsync_PaydayThrows_PropagatesException()
    {
        // Arrange (everything up to payday succeeds)
        var trainer  = new TrainerDto { Id = Guid.NewGuid(), Ssn = "T-SSN", Name = "T", PhoneNumber = "P" };
        var program  = new ProgramDto { Id = Guid.NewGuid(), Name = "X", UnitPriceExcludingVat = 10m, UnitPriceIncludingVat = 12m, VatPercentage = 20, IsActive = true };
        var customer = new CustomerDto { Name = "C", Ssn = "C-SSN" };
        var clientId = Guid.NewGuid();
        var subscription = new SubscriptionDetailsDto { Id = Guid.NewGuid(), StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddMonths(1), DurationInMonths = 1, ClientName = "C", Program = program };

        _mockTrainerRepository.Setup(r => r.GetTrainerBySsnAsync("T-SSN")).ReturnsAsync(trainer);
        _mockProgramRepository .Setup(p => p.GetProgramByIdAsync(trainer.Id, program.Id)).ReturnsAsync(program);
        _mockTaktikalAuthService.Setup(a => a.AuthenticateAsync(It.IsAny<AuthInputModel>()))
                                    .ReturnsAsync(new EndAuthResponseModel { Authenticated = true, Customer = customer });
        _mockClientRepository   .Setup(c => c.CreateClientAsync(It.IsAny<UserInputModel>())).ReturnsAsync(clientId);
        _mockSubscriptionRepository.Setup(s => s.CreateSubscriptionAsync(It.IsAny<SubscriptionInputModel>(), trainer.Id, clientId))
                                    .ReturnsAsync(subscription);
        _mockPaydayService.Setup(p => p.CreateInvoiceAsync("T-SSN", customer.Ssn, program))
                            .ThrowsAsync(new InvalidOperationException("Payday down"));

        var input = new SubscriptionInputModel
        {
            ClientSsn         = customer.Ssn,
            StartDate         = subscription.StartDate,
            DurationInMonths  = subscription.DurationInMonths,
            ProgramId         = program.Id
        };

        // Act & Assert
        await Assert.ThrowsExceptionAsync<InvalidOperationException>(() =>
            _service.CreateSubscriptionAsync(input, "T-SSN")
        );
        _mockSubscriptionRepository.Verify(s => s.CreateSubscriptionAsync(input, trainer.Id, clientId), Times.Once);
        _mockSubscriptionRepository.Verify(s => s.CreateSubscriptionInvoiceAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never);
    }

    [TestMethod]
    public async Task CreateSubscriptionAsync_CreateInvoiceRecordThrows_PropagatesException()
    {
        // Arrange (everything up to invoice‐record creation succeeds)
        var trainer  = new TrainerDto { Id = Guid.NewGuid(), Ssn = "T-SSN", Name = "T", PhoneNumber = "P" };
        var program  = new ProgramDto { Id = Guid.NewGuid(), Name = "X", UnitPriceExcludingVat = 10m, UnitPriceIncludingVat = 12m, VatPercentage = 20, IsActive = true };
        var customer = new CustomerDto { Name = "C", Ssn = "C-SSN" };
        var clientId = Guid.NewGuid();
        var subscription = new SubscriptionDetailsDto { Id = Guid.NewGuid(), StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddMonths(1), DurationInMonths = 1, ClientName = "C", Program = program };
        var invoiceId = "INV-456";

        _mockTrainerRepository.Setup(r => r.GetTrainerBySsnAsync("T-SSN")).ReturnsAsync(trainer);
        _mockProgramRepository .Setup(p => p.GetProgramByIdAsync(trainer.Id, program.Id)).ReturnsAsync(program);
        _mockTaktikalAuthService.Setup(a => a.AuthenticateAsync(It.IsAny<AuthInputModel>()))
                                    .ReturnsAsync(new EndAuthResponseModel { Authenticated = true, Customer = customer });
        _mockClientRepository   .Setup(c => c.CreateClientAsync(It.IsAny<UserInputModel>())).ReturnsAsync(clientId);
        _mockSubscriptionRepository.Setup(s => s.CreateSubscriptionAsync(It.IsAny<SubscriptionInputModel>(), trainer.Id, clientId))
                                    .ReturnsAsync(subscription);
        _mockPaydayService.Setup(p => p.CreateInvoiceAsync("T-SSN", customer.Ssn, program))
                            .ReturnsAsync(invoiceId);
        _mockSubscriptionRepository
            .Setup(s => s.CreateSubscriptionInvoiceAsync(subscription.Id, invoiceId, 1))
            .ThrowsAsync(new Exception("DB write failed"));

        var input = new SubscriptionInputModel
        {
            ClientSsn         = customer.Ssn,
            StartDate         = subscription.StartDate,
            DurationInMonths  = subscription.DurationInMonths,
            ProgramId         = program.Id
        };

        // Act & Assert
        await Assert.ThrowsExceptionAsync<Exception>(() =>
            _service.CreateSubscriptionAsync(input, "T-SSN")
        );
        _mockPaydayService.Verify(p => p.CreateInvoiceAsync("T-SSN", customer.Ssn, program), Times.Once);
        _mockSubscriptionRepository.Verify(s => s.CreateSubscriptionInvoiceAsync(subscription.Id, invoiceId, 1), Times.Once);
    }
}
