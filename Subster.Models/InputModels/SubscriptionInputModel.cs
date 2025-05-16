using System.ComponentModel.DataAnnotations;

namespace Subster.Models.InputModels;

public class SubscriptionInputModel
{
    [Required]
    public Guid ProgramId { get; set; }
    public string? ClientSsn { get; set; }
    public string? ClientPhoneNumber { get; set; }
    [Required]
    public DateTime StartDate { get; set; }
    [Required]
    [Range(1, 12)]
    public int DurationInMonths { get; set; }
}