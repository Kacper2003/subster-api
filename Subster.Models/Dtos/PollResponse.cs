namespace Subster.Models.Dtos;

public class PollResponse
{
    public bool WaitingForUserInput { get; set; }
    public string StatusMessage { get; set; } = null!;
    public CustomerDto Customer { get; set; } = null!;
}
