using MyOS.Modules.Storage.Application.Errors;
using MyOS.Modules.Storage.Application.Files.BusinesRules;
using MyOS.UnitTests.Common;

namespace MyOS.UnitTests.Application.Storage.Files.BusinessRules
{
    public class FileTypeMustBeAllowedRuleTests
    {
        [Fact]
        public async Task CheckAsync_NullType_Fails() =>
            (await new FileTypeMustBeAllowedRule(null).CheckAsync(CancellationToken.None)).ShouldBeFalse();

        [Fact]
        public async Task CheckAsync_InactiveType_Fails() =>
            (await new FileTypeMustBeAllowedRule(StorageTestData.AllowedType(isActive: false))
                .CheckAsync(CancellationToken.None)).ShouldBeFalse();

        [Fact]
        public async Task CheckAsync_ActiveType_Passes() =>
            (await new FileTypeMustBeAllowedRule(StorageTestData.AllowedType(isActive: true))
                .CheckAsync(CancellationToken.None)).ShouldBeTrue();

        [Fact]
        public void Error_IsTypeNotAllowed() =>
            new FileTypeMustBeAllowedRule(null).Error.ShouldBe(FileErrors.TypeNotAllowed);
    }
}
