using Serilog;

namespace SmartTaskManager.Api.Logging;

public static class SerilogExtensions
{
    // Sinks and levels come from the "Serilog" configuration section, so each environment can change them
    // without code. Per-request lines come from RequestLoggingMiddleware, which never logs bodies or headers.
    public static WebApplicationBuilder AddSerilogLogging(this WebApplicationBuilder builder)
    {
        builder.Services.AddSerilog(
            (services, logger) =>
                logger
                    .ReadFrom.Configuration(builder.Configuration)
                    .ReadFrom.Services(services)
                    .Enrich.FromLogContext()
        );

        return builder;
    }
}
