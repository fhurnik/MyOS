using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using MyOS.Core.Application.Abstractions;
using MyOS.Modules.Fitness.Application.Resources;

namespace MyOS.Modules.Fitness.Application.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddFitnessApplication(this IServiceCollection services)
        {
            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

            services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

            services.AddSingleton<IErrorMessageProvider, FitnessErrorMessageProvider>();

            return services;
        }
    }
}
