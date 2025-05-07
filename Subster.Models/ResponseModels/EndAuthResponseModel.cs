using Subster.Models.Dtos;

namespace Subster.Models.ResponseModels;

public class EndAuthResponseModel
{
    /// <summary>
    /// True if authentication succeeded.
    /// </summary>
    public bool Authenticated { get; set; }
    
    /// <summary>
    /// Contains customer data from Taktikal (if available).
    /// </summary>
    public CustomerDto Customer { get; set; } = new();
    
    /// <summary>
    /// In case of an error, this contains the error message.
    /// </summary>
    public string Error { get; set; } = "";
    
    /// <summary>
    /// The HTTP status code received from Taktikal.
    /// </summary>
    public int StatusCode { get; set; }
}
