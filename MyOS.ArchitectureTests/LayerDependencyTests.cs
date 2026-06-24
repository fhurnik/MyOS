using System.Reflection;
using NetArchTest.Rules;

namespace MyOS.ArchitectureTests
{
    // Enforces the layer-dependency and module-isolation rules from CLAUDE.md.
    // These tests are declarative: add a forbidden reference anywhere and the matching
    // test turns red, naming the offending type.
    public class LayerDependencyTests
    {
        private const string EfCore = "Microsoft.EntityFrameworkCore";
        private const string AspNetCore = "Microsoft.AspNetCore";

        private static readonly string[] DomainAssemblies =
        [
            "MyOS.Core.Domain",
            "MyOS.Identity.Domain",
            "MyOS.Modules.Notes.Domain",
            "MyOS.Modules.Storage.Domain",
            "MyOS.Modules.Fitness.Domain",
        ];

        private static readonly string[] ApplicationAssemblies =
        [
            "MyOS.Core.Application",
            "MyOS.Identity.Application",
            "MyOS.Modules.Notes.Application",
            "MyOS.Modules.Storage.Application",
            "MyOS.Modules.Fitness.Application",
        ];

        // module name -> (Domain assembly, Application assembly). Identity predates the
        // MyOS.Modules.* convention, hence the special-cased names.
        private static readonly string[] Modules = ["Identity", "Notes", "Storage", "Fitness"];

        public static TheoryData<string> DomainAssemblyNames() => ToTheory(DomainAssemblies);
        public static TheoryData<string> ApplicationAssemblyNames() => ToTheory(ApplicationAssemblies);

        [Theory]
        [MemberData(nameof(DomainAssemblyNames))]
        public void Domain_should_not_depend_on_ef_core_or_aspnet(string assemblyName)
        {
            var result = Types.InAssembly(Assembly.Load(assemblyName))
                .Should()
                .NotHaveDependencyOnAny(EfCore, AspNetCore)
                .GetResult();

            result.IsSuccessful.ShouldBeTrue(Describe(assemblyName, result));
        }

        [Theory]
        [MemberData(nameof(ApplicationAssemblyNames))]
        public void Application_should_not_depend_on_ef_core(string assemblyName)
        {
            var result = Types.InAssembly(Assembly.Load(assemblyName))
                .Should()
                .NotHaveDependencyOn(EfCore)
                .GetResult();

            result.IsSuccessful.ShouldBeTrue(Describe(assemblyName, result));
        }

        [Fact]
        public void Modules_should_not_depend_on_each_other()
        {
            foreach (var module in Modules)
            {
                var otherModuleNamespaces = Modules
                    .Where(m => m != module)
                    // covers both naming schemes: MyOS.Modules.{m}.* and legacy MyOS.{m}.*
                    .SelectMany(m => new[] { $"MyOS.Modules.{m}", $"MyOS.{m}" })
                    .ToArray();

                foreach (var assemblyName in AssemblyNamesFor(module))
                {
                    var result = Types.InAssembly(Assembly.Load(assemblyName))
                        .Should()
                        .NotHaveDependencyOnAny(otherModuleNamespaces)
                        .GetResult();

                    result.IsSuccessful.ShouldBeTrue(Describe(assemblyName, result));
                }
            }
        }

        [Fact]
        public void Analysis_actually_inspects_types_sanity_check()
        {
            // Positive control: Fitness Application genuinely depends on Core.Application.
            // If this ever comes back empty, NetArchTest isn't loading types and every
            // NotHaveDependencyOn rule above would be passing vacuously.
            var dependentTypes = Types.InAssembly(Assembly.Load("MyOS.Modules.Fitness.Application"))
                .That()
                .HaveDependencyOn("MyOS.Core.Application")
                .GetTypes();

            dependentTypes.ShouldNotBeEmpty();
        }

        private static IEnumerable<string> AssemblyNamesFor(string module) =>
            module == "Identity"
                ? ["MyOS.Identity.Domain", "MyOS.Identity.Application"]
                : [$"MyOS.Modules.{module}.Domain", $"MyOS.Modules.{module}.Application"];

        private static TheoryData<string> ToTheory(string[] names)
        {
            var data = new TheoryData<string>();
            foreach (var name in names)
                data.Add(name);
            return data;
        }

        private static string Describe(string assemblyName, TestResult result) =>
            result.IsSuccessful
                ? string.Empty
                : $"{assemblyName} has forbidden dependencies. Offending types: " +
                  string.Join(", ", result.FailingTypeNames ?? []);
    }
}
