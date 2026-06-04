using System.Reflection;
using Serilog.Core;
using Serilog.Events;

namespace MyOS.Core.Infrastructure.Logging;

public sealed class SensitiveDataDestructuringPolicy : IDestructuringPolicy
{
    private static readonly HashSet<string> SensitiveProperties = new(StringComparer.OrdinalIgnoreCase)
    {
        "Password", "Token", "RefreshToken", "Secret", "SecretKey", "Key", "Hash", "PasswordHash"
    };

    public bool TryDestructure(object value, ILogEventPropertyValueFactory propertyValueFactory, out LogEventPropertyValue result)
    {
        var type = value.GetType();
        if (!type.IsClass || type == typeof(string))
        {
            result = null!;
            return false;
        }

        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        if (!properties.Any(p => SensitiveProperties.Contains(p.Name)))
        {
            result = null!;
            return false;
        }

        var logProperties = properties.Select(p => new LogEventProperty(
            p.Name,
            SensitiveProperties.Contains(p.Name)
                ? new ScalarValue("[REDACTED]")
                : propertyValueFactory.CreatePropertyValue(p.GetValue(value), true)));

        result = new StructureValue(logProperties);
        return true;
    }
}
