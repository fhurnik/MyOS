using Microsoft.Extensions.DependencyInjection;
using MyOS.Core.Infrastructure.EntityFrameworkConfiguration;
using MyOS.Identity.Infrastructure.EntityConfigurations.Users;

namespace MyOS.Identity.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddIdentityModule(this IServiceCollection services)
        {
            services.AddEfConfigurationsFromAssembly(typeof(UserEntityConfiguration).Assembly);

            return services;
        }
    }
}
