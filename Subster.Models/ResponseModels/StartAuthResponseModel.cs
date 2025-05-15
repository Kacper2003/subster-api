namespace Subster.Models.ResponseModels;

public class StartAuthResponseModel
{
    public string AuthRequestId { get; set; } = null!;
    public string VerificationCode { get; set; } = null!;
    public int PollingInterval { get; set; }
}
