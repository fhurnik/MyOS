using MediatR;
using Microsoft.Extensions.DependencyInjection;
using MyOS.Core.Application.Abstractions;
using MyOS.Core.Application.Behaviors;
using MyOS.Core.Application.Resources;

namespace MyOS.Core.Application.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddCoreApplication(this IServiceCollection services)
        {
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            services.AddSingleton<IErrorMessageProvider, CoreErrorMessageProvider>();

            return services;
        }
    }
}
