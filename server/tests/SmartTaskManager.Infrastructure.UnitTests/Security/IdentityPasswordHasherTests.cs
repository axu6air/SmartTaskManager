using FluentAssertions;
using SmartTaskManager.Infrastructure.Security;

namespace SmartTaskManager.Infrastructure.UnitTests.Security;

public class IdentityPasswordHasherTests
{
    private readonly IdentityPasswordHasher _hasher = new();

    [Fact]
    public void Verify_CorrectPassword_ReturnsTrue()
    {
        var hash = _hasher.Hash("password123");

        _hasher.Verify(hash, "password123").Should().BeTrue();
    }

    [Fact]
    public void Verify_WrongPassword_ReturnsFalse()
    {
        var hash = _hasher.Hash("password123");

        _hasher.Verify(hash, "password124").Should().BeFalse();
    }

    [Fact]
    public void Hash_SamePasswordTwice_ProducesDifferentHashes()
    {
        _hasher.Hash("password123").Should().NotBe(_hasher.Hash("password123"));
    }
}
