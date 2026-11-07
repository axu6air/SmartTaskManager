using FluentAssertions;
using SmartTaskManager.Application.Features.Auth.Commands.Login;

namespace SmartTaskManager.Application.UnitTests.Features.Auth;

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator = new();

    [Fact]
    public void Validate_EmailAndPassword_IsValid()
    {
        _validator.Validate(new LoginCommand("member@email.com", "password123", null)).IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "password123")]
    [InlineData("not-an-email", "password123")]
    [InlineData("member@email.com", "")]
    public void Validate_MissingOrMalformedInput_IsInvalid(string email, string password)
    {
        _validator.Validate(new LoginCommand(email, password, null)).IsValid.Should().BeFalse();
    }
}
