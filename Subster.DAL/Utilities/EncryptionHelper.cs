using Microsoft.AspNetCore.DataProtection;

// namespace Subster.DAL.Utilities;

// public class EncryptionHelper
// {
//     private readonly IDataProtector _protector;

//     public EncryptionHelper(IDataProtectionProvider provider)
//     {
//         _protector = provider.CreateProtector("Subster.SensitiveData");
//     }

//     public string Protect(string input) => _protector.Protect(input);

//     public string Unprotect(string encryptedInput) => _protector.Unprotect(encryptedInput);
// }
namespace Subster.DAL.Utilities;

public class EncryptionHelper
{
    public EncryptionHelper(IDataProtectionProvider _) { }

    public string Protect(string input) => input;

    public string Unprotect(string encryptedInput) => encryptedInput;
}
