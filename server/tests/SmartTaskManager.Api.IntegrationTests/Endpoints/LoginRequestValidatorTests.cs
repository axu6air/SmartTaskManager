using FluentAssertions;
using SmartTaskManager.Api.Endpoints.Auth;

namespace SmartTaskManager.Api.IntegrationTests.Endpoints;

public class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _validator = new();

    [Fact]
    public void Validate_EmailAndPassword_IsValid()
    {
        _validator.Validate(new LoginRequest("member@email.com", "password123")).IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "password123")]
    [InlineData("not-an-email", "password123")]
    [InlineData("member@email.com", "")]
    [InlineData(null, "password123")]
    [InlineData("member@email.com", null)]
    public void Validate_MissingOrMalformedInput_IsInvalid(string? email, string? password)
    {
        _validator.Validate(new LoginRequest(email!, password!)).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_OverlongPassword_IsInvalid()
    {
        _validator.Validate(new LoginRequest("member@email.com", new string('a', 129))).IsValid.Should().BeFalse();
    }
}
