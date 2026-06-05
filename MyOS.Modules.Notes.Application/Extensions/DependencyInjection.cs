using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using MyOS.Core.Application.Abstractions;
using MyOS.Modules.Notes.Application.Resources;

namespace MyOS.Modules.Notes.Application.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddNotesApplication(this IServiceCollection services)
        {
            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

            services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

            services.AddSingleton<IErrorMessageProvider, NotesErrorMessageProvider>();

            return services;
        }
    }
}
