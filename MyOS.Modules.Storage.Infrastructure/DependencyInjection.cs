using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyOS.Core.Infrastructure.EntityFrameworkConfiguration;
using MyOS.Modules.Storage.Application.Extensions;
using MyOS.Modules.Storage.Domain.Quotas;
using MyOS.Modules.Storage.Infrastructure.EntityConfigurations.Quotas;
using MyOS.Modules.Storage.Infrastructure.Repositories;

namespace MyOS.Modules.Storage.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddStorageModule(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddEfConfigurationsFromAssembly(typeof(StorageQuotaEntityConfiguration).Assembly);

            services.AddScoped<IStorageQuotaRepository, StorageQuotaRepository>();

            services.AddStorageApplication();

            return services;
        }
    }
}
