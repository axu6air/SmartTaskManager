using Microsoft.AspNetCore.Identity;
using SmartTaskManager.Application.Common.Abstractions;

namespace SmartTaskManager.Infrastructure.Security;

public class IdentityPasswordHasher : IPasswordHasher
{
    // The default PasswordHasher ignores the user argument, so one shared placeholder is enough.
    private static readonly object Subject = new();

    private readonly PasswordHasher<object> _hasher = new();

    public string Hash(string password) => _hasher.HashPassword(Subject, password);

    public bool Verify(string passwordHash, string password)
    {
        var result = _hasher.VerifyHashedPassword(Subject, passwordHash, password);

        if (result == PasswordVerificationResult.Failed)
        {
            return false;
        }

        return true;
    }
}
