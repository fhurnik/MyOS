using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace MyOS.Core.Infrastructure.EntityFrameworkConfiguration
{
    public sealed class AppDbContext : DbContext
    {
        private readonly EfModuleOptions _efModuleOptions;
        public AppDbContext(DbContextOptions<AppDbContext> options, IOptions<EfModuleOptions> efModuleOptions) : base(options)
        {
            _efModuleOptions = efModuleOptions.Value;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            foreach (var assembly in _efModuleOptions.ConfigurationAssemblies)
                modelBuilder.ApplyConfigurationsFromAssembly(assembly);

            base.OnModelCreating(modelBuilder);
        }
    }
}
