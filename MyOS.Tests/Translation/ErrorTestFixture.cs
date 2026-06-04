using MyOS.Core.Application.Abstractions;
using MyOS.Core.Application.Abstractions.Results;
using MyOS.Identity.Application.Resources;
using System.Reflection;

namespace MyOS.Tests.Translation
{
    internal static class ErrorTestFixture
    {
        public static readonly Assembly[] ModuleAssemblies =
        [
            typeof(IdentityErrorMessageProvider).Assembly
        ];

        public static readonly IErrorMessageProvider[] Providers =
        [
            new IdentityErrorMessageProvider()
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
