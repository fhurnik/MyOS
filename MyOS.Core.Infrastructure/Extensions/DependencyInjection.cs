using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyOS.Core.Application.Extensions;

namespace MyOS.Core.Infrastructure.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddCore(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddCoreApplication();

            return services;
        }
    }
}
