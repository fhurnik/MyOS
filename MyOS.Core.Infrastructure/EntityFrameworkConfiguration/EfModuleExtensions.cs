using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace MyOS.Core.Infrastructure.EntityFrameworkConfiguration
{
    public static class EfModuleExtensions
    {
        public static IServiceCollection AddEfConfigurationsFromAssembly(this IServiceCollection services, Assembly assembly)
        {
            services.Configure<EfModuleOptions>(options =>
            {
                if (!options.ConfigurationAssemblies.Contains(assembly))
                {
                    options.ConfigurationAssemblies.Add(assembly);
                }
            });

            return services;
        }
    }
}
