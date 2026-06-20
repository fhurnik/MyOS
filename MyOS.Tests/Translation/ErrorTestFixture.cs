using MyOS.Core.Application.Abstractions;
using MyOS.Core.Application.Abstractions.Results;
using MyOS.Core.Application.Resources;
using MyOS.Identity.Application.Resources;
using MyOS.Modules.Notes.Application.Resources;
using MyOS.Modules.Storage.Application.Resources;
using MyOS.Modules.Fitness.Application.Resources;
using System.Reflection;

namespace MyOS.Tests.Translation
{
    internal static class ErrorTestFixture
    {
        public static readonly Assembly[] ModuleAssemblies =
        [
            typeof(CoreErrorMessageProvider).Assembly,
            typeof(IdentityErrorMessageProvider).Assembly,
            typeof(NotesErrorMessageProvider).Assembly,
            typeof(StorageErrorMessageProvider).Assembly,
            typeof(FitnessErrorMessageProvider).Assembly
        ];

        public static readonly IErrorMessageProvider[] Providers =
        [
            new CoreErrorMessageProvider(),
            new IdentityErrorMessageProvider(),
            new NotesErrorMessageProvider(),
            new StorageErrorMessageProvider(),
            new FitnessErrorMessageProvider()
        ];

        public static IEnumerable<(Assembly Assembly, string ResourceName)> ResourceManifests =>
            ModuleAssemblies.SelectMany(a =>
                a.GetManifestResourceNames()
                    .Where(n => n.EndsWith(".resources"))
                    .Select(n => (a, n)));

        public static IEnumerable<(Type Type, FieldInfo Field, Error Error)> AllErrorFields() =>
            ModuleAssemblies
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsSubclassOf(typeof(ErrorCodes)) && !t.IsAbstract)
                .SelectMany(t => t.GetFields(BindingFlags.Public | BindingFlags.Static)
                    .Where(f => f.FieldType == typeof(Error))
                    .Select(f => (t, f, (Error)f.GetValue(null)!)));
    }
}
