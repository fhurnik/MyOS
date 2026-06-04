using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyOS.Core.Application.Abstractions;
using MyOS.Core.Application.Extensions;
using MyOS.Core.Infrastructure.EntityFrameworkConfiguration;
using MyOS.Core.Infrastructure.Persistence;
using MyOS.Core.Infrastructure.Services;

namespace MyOS.Core.Infrastructure.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddCore(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddCoreApplication();

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("Database")));

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUser, CurrentUserService>();

            return services;
        }
    }
}
