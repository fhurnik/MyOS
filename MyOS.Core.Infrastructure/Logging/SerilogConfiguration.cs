using Serilog;
using Serilog.Events;

namespace MyOS.Core.Infrastructure.Logging
{
    public static class SerilogConfiguration
    {
        public static LoggerConfiguration ConfigureSerilog(this LoggerConfiguration loggerConfiguration)
        {
            return loggerConfiguration
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Destructure.With<SensitiveDataDestructuringPolicy>()
                .WriteTo.Console();
        }

    }
}
