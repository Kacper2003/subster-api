namespace Subster.Models.Dtos;

public class SubscriptionDto
{
    public int Id { get; set; }
    public string ClientName { get; set; } = "";
    public string ClientSsn { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}