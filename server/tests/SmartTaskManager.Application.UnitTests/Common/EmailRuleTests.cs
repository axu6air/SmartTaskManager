using FluentAssertions;
using SmartTaskManager.Application.Features.Auth.Commands.Login;
using SmartTaskManager.Application.Features.Auth.Commands.Register;

namespace SmartTaskManager.Application.UnitTests.Common;

public class EmailRuleTests
{
    [Theory]
    [InlineData("name@company.com")]
    [InlineData("first.last+tag@mail.company.co.uk")]
    [InlineData("MEMBER11@EMAIL.COM")]
    public void ValidEmail_WellFormed_Passes(string email)
    {
        var result = new RegisterUserCommandValidator().Validate(new RegisterUserCommand("Name", email, "Secret123", null));

        result.Errors.Should().NotContain(e => e.PropertyName == nameof(RegisterUserCommand.Email));
    }

    [Theory]
    [InlineData("name@company")] // no domain ending: FluentValidation's EmailAddress() accepts this
    [InlineData("name@company.c")] // ending too short
    [InlineData("name@@company.com")]
    [InlineData("name company@x.com")]
    [InlineData("@company.com")]
    [InlineData("name@.com")]
    [InlineData("plainaddress")]
    public void ValidEmail_Malformed_IsRejectedOnRegisterAndLogin(string email)
    {
        var register = new RegisterUserCommandValidator().Validate(new RegisterUserCommand("Name", email, "Secret123", null));
        var login = new LoginCommandValidator().Validate(new LoginCommand(email, "password", null));

        register.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterUserCommand.Email));
        login.Errors.Should().Contain(e => e.PropertyName == nameof(LoginCommand.Email));
    }
}
