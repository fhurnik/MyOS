using System.Reflection;

namespace MyOS.Core.Infrastructure.EntityFrameworkConfiguration
{
    public sealed class EfModuleOptions
    {
        public List<Assembly> ConfigurationAssemblies { get; set; } = [];
    }
}
