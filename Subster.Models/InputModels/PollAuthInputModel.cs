namespace Subster.Models.InputModels;

public class PollAuthInputModel
{
    public string? AuthRequestId { get; set; }
    public string FlowKey { get; set; } = null!;

    // Has to be "Name", defauly get's more details about the person and costs
    public string LookupType { get; set; } = "Name";
}