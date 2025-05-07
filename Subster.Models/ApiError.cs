namespace Subster.Models;

public record ApiError
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = null!;
}
