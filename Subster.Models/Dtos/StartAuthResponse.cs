namespace Subster.Models.Dtos;

public class StartAuthResponse
{
    public string AuthRequestId { get; set; }
    public string VerificationCode { get; set; }
    public int PollingInterval { get; set; }
}
