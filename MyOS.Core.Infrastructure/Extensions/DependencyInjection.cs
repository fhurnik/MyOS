using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyOS.Core.Application.Abstractions;
using MyOS.Core.Application.Extensions;
using MyOS.Core.Infrastructure.EntityFrameworkConfiguration;
using MyOS.Core.Infrastructure.Persistence;
using MyOS.Core.Infrastructure.Services;
using SqlKata.Compilers;
using SqlKata.Execution;

namespace MyOS.Core.Infrastructure.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddCore(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddCoreApplication();

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("Database")));

            services.AddScoped<QueryFactory>(sp =>
            {
                var connectionString = configuration.GetConnectionString("Database")!;
                var factory = new QueryFactory(new SqlConnection(connectionString), new SqlServerCompiler());

                var logger = sp.GetRequiredService<ILogger<QueryFactory>>();
                factory.Logger = result =>
                    logger.LogDebug("SQLKata: {Sql}", result.Sql);

                return factory;
            });

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUser, CurrentUserService>();

            services.AddScoped<IErrorTranslator, ErrorTranslator>();

            return services;
        }
    }
}
