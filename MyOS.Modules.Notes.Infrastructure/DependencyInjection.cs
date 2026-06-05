using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyOS.Core.Infrastructure.EntityFrameworkConfiguration;
using MyOS.Modules.Notes.Application.Extensions;
using MyOS.Modules.Notes.Domain.Notes.CheckList;
using MyOS.Modules.Notes.Domain.Notes.TextNotes;
using MyOS.Modules.Notes.Infrastructure.EntityConfigurations.TextNotes;
using MyOS.Modules.Notes.Infrastructure.Repositories;

namespace MyOS.Modules.Notes.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddNotesModule(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddEfConfigurationsFromAssembly(typeof(TextNoteEntityConfiguration).Assembly);

            services.AddScoped<ITextNoteRepository, TextNoteRepository>();
            services.AddScoped<ICheckListRepository, CheckListRepository>();

            services.AddNotesApplication();

            return services;
        }
    }
}
