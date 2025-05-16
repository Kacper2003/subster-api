using Subster.Models.Dtos;

namespace Subster.Models.ResponseModels;

public class EndAuthResponseModel
{
    public bool Authenticated { get; set; }
    public CustomerDto Customer { get; set; } = new();
    public string? Error { get; set; }
    public int StatusCode { get; set; }
}
