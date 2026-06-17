using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyOS.Core.Infrastructure.EntityFrameworkConfiguration;
using MyOS.Modules.Storage.Application.Abstractions;
using MyOS.Modules.Storage.Application.Extensions;
using MyOS.Modules.Storage.Domain.AllowedFileTypes;
using MyOS.Modules.Storage.Domain.Files;
using MyOS.Modules.Storage.Domain.Folders;
using MyOS.Modules.Storage.Domain.Quotas;
using MyOS.Modules.Storage.Infrastructure.EntityConfigurations.Quotas;
using MyOS.Modules.Storage.Infrastructure.Repositories;
using MyOS.Modules.Storage.Infrastructure.Services;
using MyOS.Modules.Storage.Infrastructure.Settings;

namespace MyOS.Modules.Storage.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddStorageModule(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddEfConfigurationsFromAssembly(typeof(StorageQuotaEntityConfiguration).Assembly);

            services.Configure<StorageSettings>(configuration.GetSection(StorageSettings.SectionName));

            services.AddScoped<IStorageQuotaRepository, StorageQuotaRepository>();
            services.AddScoped<IStoredFileRepository, StoredFileRepository>();
            services.AddScoped<IAllowedFileTypeRepository, AllowedFileTypeRepository>();
            services.AddScoped<IFolderRepository, FolderRepository>();

            services.AddScoped<IFileStorage, LocalFileStorage>();
            services.AddSingleton<IFileSignatureValidator, FileSignatureValidator>();

            services.AddStorageApplication();

            return services;
        }
    }
}
