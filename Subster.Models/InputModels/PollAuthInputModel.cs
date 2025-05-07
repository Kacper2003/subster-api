namespace Subster.Models.InputModels;

public class PollAuthInputModel
{
    public string? AuthRequestId { get; set; }
    public string FlowKey { get; set; } = null!;
    public string LookupType { get; set; } = "Name"; // IMPORTANT
}