using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using SmartTaskManager.Infrastructure.Persistence;

namespace SmartTaskManager.Infrastructure.UnitTests.Persistence;

public class DatabaseOptionsTests
{
    private static bool IsValid(DatabaseOptions options) =>
        Validator.TryValidateObject(options, new ValidationContext(options), [], validateAllProperties: true);

    [Fact]
    public void Validate_DefaultsWithConnectionString_IsValid()
    {
        IsValid(new DatabaseOptions { ConnectionString = "Server=.;Database=Db" }).Should().BeTrue();
    }

    [Theory]
    [InlineData(-1, 30, 500)]
    [InlineData(5, 0, 500)]
    [InlineData(5, 30, 0)]
    public void Validate_OutOfRangeValue_IsInvalid(int retryCount, int retryDelaySeconds, int slowQueryThreshold)
    {
        var options = new DatabaseOptions
        {
            ConnectionString = "Server=.;Database=Db",
            MaxRetryCount = retryCount,
            MaxRetryDelaySeconds = retryDelaySeconds,
            SlowQueryThreshold = slowQueryThreshold,
        };

        IsValid(options).Should().BeFalse();
    }
}
