using Microsoft.AspNetCore.DataProtection;

namespace Subster.DAL.Utilities;

public class EncryptionHelper(IDataProtectionProvider provider)
{
    // Simple encryption helper, the IDataProtectionProvider is set up in Subster.API/Program.cs
    private readonly IDataProtector _protector = provider.CreateProtector("Subster.SensitiveData");

	public string Protect(string input) => _protector.Protect(input);

    public string Unprotect(string encryptedInput) => _protector.Unprotect(encryptedInput);
}

