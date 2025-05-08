namespace Subster.Models.Dtos.Payday;

public class PaydayTokenResponse
{
    public string? AccessToken { get; set; }
    public string? TokenType { get; set; }
    public int ExpiresIn { get; set; }
    public int CreatedAt { get; set; }
}