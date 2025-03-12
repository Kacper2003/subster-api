namespace Subster.Models.Dtos;

public class PollResponse
{
    public bool WaitingForUserInput { get; set; }
    public string StatusMessage { get; set; }
    public CustomerDto Customer { get; set; }
}
