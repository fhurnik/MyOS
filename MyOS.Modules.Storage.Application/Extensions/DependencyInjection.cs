using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using MyOS.Core.Application.Abstractions;
using MyOS.Modules.Storage.Application.Resources;

namespace MyOS.Modules.Storage.Application.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddStorageApplication(this IServiceCollection services)
        {
            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

            services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

            services.AddSingleton<IErrorMessageProvider, StorageErrorMessageProvider>();

            return services;
        }
    }
}
