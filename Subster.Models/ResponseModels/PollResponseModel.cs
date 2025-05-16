using Subster.Models.Dtos;

namespace Subster.Models.ResponseModels;

public class PollResponseModel
{
    public bool WaitingForUserInput { get; set; }
    public string StatusMessage { get; set; } = null!;
    public CustomerDto Customer { get; set; } = null!;
}
