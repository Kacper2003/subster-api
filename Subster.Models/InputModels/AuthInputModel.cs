using System.ComponentModel.DataAnnotations;

namespace Subster.Models.InputModels;

public class AuthInputModel
{
    [Length(7, 7)]
    public string? PhoneNumber { get; set; }
    [Length(10, 10)]
    public string? Ssn { get; set; }
}