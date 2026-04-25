using MediatR;
using Microsoft.Extensions.DependencyInjection;
using MyOS.Core.Application.Behaviors;

namespace MyOS.Core.Application.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddCoreApplication(this IServiceCollection services)
        {
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            return services;
        }
    }
}
