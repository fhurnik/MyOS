using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyOS.Core.Infrastructure.EntityFrameworkConfiguration;
using MyOS.Modules.Fitness.Application.Extensions;
using MyOS.Modules.Fitness.Domain.Exercises;
using MyOS.Modules.Fitness.Infrastructure.EntityConfigurations.Exercises;
using MyOS.Modules.Fitness.Infrastructure.Repositories;

namespace MyOS.Modules.Fitness.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddFitnessModule(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddEfConfigurationsFromAssembly(typeof(ExerciseEntityConfiguration).Assembly);

            services.AddScoped<IExerciseRepository, ExerciseRepository>();

            services.AddFitnessApplication();

            return services;
        }
    }
}
