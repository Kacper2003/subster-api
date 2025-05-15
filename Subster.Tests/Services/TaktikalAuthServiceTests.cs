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
    private Mock<ITaktikalApiClient> _mockClient = null!;  // fake HTTP client
    private IConfiguration            _config     = null!;  // in-memory config
    private TaktikalAuthService      _service    = null!;  // service under test

    [TestInitialize]
    public void Setup()
    {
        // Prepare mocked API client
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
        // Arrange: missing both identifiers
        var input = new AuthInputModel
        {
            PhoneNumber = null,
            Ssn         = null
        };

        // Act
        var result = await _service.AuthenticateAsync(input);

        // Assert: should reject with bad request
        Assert.IsFalse(result.Authenticated);
        Assert.AreEqual(400, result.StatusCode);
        StringAssert.Contains(result.Error, "Either PhoneNumber or Ssn");
    }

    [TestMethod]
    public async Task AuthenticateAsync_MissingFlowKey_Returns500()
    {
        // Arrange: no FlowKey in config
        var emptyConfig = new ConfigurationBuilder().Build();
        var svc = new TaktikalAuthService(_mockClient.Object, emptyConfig);
        var input = new AuthInputModel { PhoneNumber = "555" };

        // Act
        var result = await svc.AuthenticateAsync(input);

        // Assert: should return server error
        Assert.IsFalse(result.Authenticated);
        Assert.AreEqual(500, result.StatusCode);
        StringAssert.Contains(result.Error, "FlowKey is missing");
    }

    [TestMethod]
    public async Task AuthenticateAsync_StartAsyncReturnsNull_Returns502()
    {
        // Arrange: StartAsync fails
        var input = new AuthInputModel
        {
            PhoneNumber = "555-1234",
            Ssn         = null
        };
        _mockClient
            .Setup(c => c.StartAsync(It.IsAny<StartAuthInputModel>()))
            .Returns(Task.FromResult<StartAuthResponseModel?>(null));

        // Act
        var result = await _service.AuthenticateAsync(input);

        // Assert: should return bad gateway
        Assert.IsFalse(result.Authenticated);
        Assert.AreEqual(502, result.StatusCode);
        StringAssert.Contains(result.Error, "Failed to call /auth/start");

        // Verify StartAsync called with correct DTO
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
        // Arrange: StartAsync succeeds, PollAsync fails
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
        var result = await _service.AuthenticateAsync(input);

        // Assert: should return bad gateway on poll failure
        Assert.IsFalse(result.Authenticated);
        Assert.AreEqual(502, result.StatusCode);
        StringAssert.Contains(result.Error, "Failed to call /auth/poll");

        // Verify both StartAsync and PollAsync were invoked
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
        // Arrange: both StartAsync and PollAsync succeed
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
        var result = await _service.AuthenticateAsync(input);

        // Assert: should return success with correct customer
        Assert.IsTrue(result.Authenticated);
        Assert.AreEqual(200, result.StatusCode);
        Assert.AreEqual(string.Empty, result.Error);
        Assert.IsNotNull(result.Customer);
        Assert.AreEqual(expectedCustomer.Name, result.Customer!.Name);
        Assert.AreEqual(expectedCustomer.Ssn,  result.Customer.Ssn);

        // Verify both client methods were called once
        _mockClient.Verify(c => c.StartAsync(It.IsAny<StartAuthInputModel>()), Times.Once);
        _mockClient.Verify(c => c.PollAsync(It.IsAny<PollAuthInputModel>()),   Times.Once);
    }

    [TestMethod]
    public async Task AuthenticateAsync_SsnOnly_UsesAppContextType()
    {
        // Arrange: SSN branch
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

        // Assert: StartAsync should use "App" context
        _mockClient.Verify(c => c.StartAsync(
            It.Is<StartAuthInputModel>(dto => dto.AuthenticationContextType == "App")
        ), Times.Once);
    }

    [TestMethod]
    public async Task AuthenticateAsync_PhoneOnly_UsesSimContextType()
    {
        // Arrange: Phone branch
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

        // Assert: StartAsync should use "Sim" context
        _mockClient.Verify(c => c.StartAsync(
            It.Is<StartAuthInputModel>(dto => dto.AuthenticationContextType == "Sim")
        ), Times.Once);
    }
}
