namespace Subster.Models.InputModels;

public class StartAuthInputModel
{
    public string? PhoneNumber { get; set; }
    public string? Ssn { get; set; }
    public string FlowKey { get; set; } = null!;
    public string AuthenticationContextType { get; set; } = null!;
}