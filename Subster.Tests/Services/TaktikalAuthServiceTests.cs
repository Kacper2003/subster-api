using Moq;
using Microsoft.Extensions.Configuration;
using Subster.API.Clients;
using Subster.API.Services.Implementations;
using Subster.Models.InputModels;
using Subster.Models.ResponseModels;
using Subster.Models.Dtos;

namespace Subster.Tests.Services;

[TestClass]
public class TaktikalAuthServiceTests
{
    private Mock<ITaktikalApiClient> _mockClient = null!;
    private IConfiguration            _config     = null!;
    private TaktikalAuthService      _service    = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockClient = new Mock<ITaktikalApiClient>();

        // Provide FlowKey via in-memory configuration
        var settings = new Dictionary<string, string?>
        {
            ["Taktikal:FlowKey"] = "dummy-flow-key"
        };
        _config = new ConfigurationBuilder()
                    .AddInMemoryCollection(settings)
                    .Build();

        // Instantiate service with mocks
        _service = new TaktikalAuthService(_mockClient.Object, _config);
    }

    [TestMethod]
    public async Task AuthenticateAsync_NoPhoneOrSsn_Returns400()
    {
        // Arrange
        var input = new AuthInputModel
        {
            PhoneNumber = null,
            Ssn         = null
        };

		// Act
		EndAuthResponseModel result = await _service.AuthenticateAsync(input);

        // Assert
        Assert.IsFalse(result.Authenticated);
        Assert.AreEqual(400, result.StatusCode);
        StringAssert.Contains(result.Error, "Either PhoneNumber or Ssn");
    }

    [TestMethod]
    public async Task AuthenticateAsync_MissingFlowKey_Returns500()
    {
		// Arrange
		IConfigurationRoot emptyConfig = new ConfigurationBuilder().Build();
        var svc = new TaktikalAuthService(_mockClient.Object, emptyConfig);
        var input = new AuthInputModel { PhoneNumber = "555" };

		// Act
		EndAuthResponseModel result = await svc.AuthenticateAsync(input);

        // Assert
        Assert.IsFalse(result.Authenticated);
        Assert.AreEqual(500, result.StatusCode);
        StringAssert.Contains(result.Error, "FlowKey is missing");
    }

    [TestMethod]
    public async Task AuthenticateAsync_StartAsyncReturnsNull_Returns502()
    {
        // Arrange
        var input = new AuthInputModel
        {
            PhoneNumber = "555-1234",
            Ssn         = null
        };
        _mockClient
            .Setup(c => c.StartAsync(It.IsAny<StartAuthInputModel>()))
            .Returns(Task.FromResult<StartAuthResponseModel?>(null));

		// Act
		EndAuthResponseModel result = await _service.AuthenticateAsync(input);

        // Assert
        Assert.IsFalse(result.Authenticated);
        Assert.AreEqual(502, result.StatusCode);
        StringAssert.Contains(result.Error, "Failed to call /auth/start");

        _mockClient.Verify(c => c.StartAsync(
            It.Is<StartAuthInputModel>(dto =>
                dto.FlowKey     == "dummy-flow-key" &&
                dto.PhoneNumber == input.PhoneNumber &&
                dto.Ssn         == input.Ssn
            )
        ), Times.Once);
    }

    [TestMethod]
    public async Task AuthenticateAsync_PollAsyncReturnsNull_Returns502()
    {
        // Arrange
        var input = new AuthInputModel { PhoneNumber = "555-1234", Ssn = null };
        var fakeStart = new StartAuthResponseModel
        {
            AuthRequestId    = "REQ-XYZ",
            VerificationCode = "CODE",
            PollingInterval  = 0
        };
        _mockClient
            .Setup(c => c.StartAsync(It.IsAny<StartAuthInputModel>()))
            .Returns(Task.FromResult<StartAuthResponseModel?>(fakeStart));
        _mockClient
            .Setup(c => c.PollAsync(It.IsAny<PollAuthInputModel>()))
            .Returns(Task.FromResult<PollResponseModel?>(null));

		// Act
		EndAuthResponseModel result = await _service.AuthenticateAsync(input);

        // Assert
        Assert.IsFalse(result.Authenticated);
        Assert.AreEqual(502, result.StatusCode);
        StringAssert.Contains(result.Error, "Failed to call /auth/poll");

        _mockClient.Verify(c => c.StartAsync(It.IsAny<StartAuthInputModel>()), Times.Once);
        _mockClient.Verify(c => c.PollAsync(
            It.Is<PollAuthInputModel>(dto =>
                dto.AuthRequestId == fakeStart.AuthRequestId &&
                dto.FlowKey       == "dummy-flow-key" &&
                dto.LookupType    == "Name"
            )
        ), Times.Once);
    }

    [TestMethod]
    public async Task AuthenticateAsync_PollingSucceeds_Returns200WithCustomer()
    {
        // Arrange
        var input = new AuthInputModel { PhoneNumber = "555-1234", Ssn = null };
        var fakeStart = new StartAuthResponseModel
        {
            AuthRequestId    = "REQ-XYZ",
            VerificationCode = "VER-001",
            PollingInterval  = 0
        };
        _mockClient
            .Setup(c => c.StartAsync(It.IsAny<StartAuthInputModel>()))
            .Returns(Task.FromResult<StartAuthResponseModel?>(fakeStart));

        var expectedCustomer = new CustomerDto { Name = "Test User", Ssn = "123-45-6789" };
        var fakePoll = new PollResponseModel
        {
            WaitingForUserInput = false,
            StatusMessage       = "OK",
            Customer            = expectedCustomer
        };
        _mockClient
            .Setup(c => c.PollAsync(It.IsAny<PollAuthInputModel>()))
            .Returns(Task.FromResult<PollResponseModel?>(fakePoll));

		// Act
		EndAuthResponseModel result = await _service.AuthenticateAsync(input);

        // Assert
        Assert.IsTrue(result.Authenticated);
        Assert.AreEqual(200, result.StatusCode);
        Assert.AreEqual(string.Empty, result.Error);
        Assert.IsNotNull(result.Customer);
        Assert.AreEqual(expectedCustomer.Name, result.Customer!.Name);
        Assert.AreEqual(expectedCustomer.Ssn,  result.Customer.Ssn);

        _mockClient.Verify(c => c.StartAsync(It.IsAny<StartAuthInputModel>()), Times.Once);
        _mockClient.Verify(c => c.PollAsync(It.IsAny<PollAuthInputModel>()),   Times.Once);
    }

    [TestMethod]
    public async Task AuthenticateAsync_SsnOnly_UsesAppContextType()
    {
        // Arrange
        var input = new AuthInputModel { PhoneNumber = null, Ssn = "999-00-0000" };
        _mockClient
            .Setup(c => c.StartAsync(It.IsAny<StartAuthInputModel>()))
            .Returns(Task.FromResult<StartAuthResponseModel?>(new StartAuthResponseModel
            {
                AuthRequestId    = "ANY",
                VerificationCode = string.Empty,
                PollingInterval  = 0
            }));
        _mockClient
            .Setup(c => c.PollAsync(It.IsAny<PollAuthInputModel>()))
            .Returns(Task.FromResult<PollResponseModel?>(new PollResponseModel
            {
                WaitingForUserInput = false,
                StatusMessage       = string.Empty,
                Customer            = new CustomerDto()
            }));

        // Act
        await _service.AuthenticateAsync(input);

        // Assert
        _mockClient.Verify(c => c.StartAsync(
            It.Is<StartAuthInputModel>(dto => dto.AuthenticationContextType == "App")
        ), Times.Once);
    }

    [TestMethod]
    public async Task AuthenticateAsync_PhoneOnly_UsesSimContextType()
    {
        // Arrange
        var input = new AuthInputModel { PhoneNumber = "444-5555", Ssn = null };
        _mockClient
            .Setup(c => c.StartAsync(It.IsAny<StartAuthInputModel>()))
            .Returns(Task.FromResult<StartAuthResponseModel?>(new StartAuthResponseModel
            {
                AuthRequestId    = "ANY",
                VerificationCode = string.Empty,
                PollingInterval  = 0
            }));
        _mockClient
            .Setup(c => c.PollAsync(It.IsAny<PollAuthInputModel>()))
            .Returns(Task.FromResult<PollResponseModel?>(new PollResponseModel
            {
                WaitingForUserInput = false,
                StatusMessage       = string.Empty,
                Customer            = new CustomerDto()
            }));

        // Act
        await _service.AuthenticateAsync(input);

        // Assert
        _mockClient.Verify(c => c.StartAsync(
            It.Is<StartAuthInputModel>(dto => dto.AuthenticationContextType == "Sim")
        ), Times.Once);
    }
}
