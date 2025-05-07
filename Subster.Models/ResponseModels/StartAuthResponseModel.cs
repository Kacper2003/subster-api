namespace Subster.Models.ResponseModels;

public class StartAuthResponseModel
{
    public string AuthRequestId { get; set; } = "";
    public string VerificationCode { get; set; } = "";
    public int PollingInterval { get; set; }
}
