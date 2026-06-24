using MyOS.Modules.Storage.Application.Errors;
using MyOS.Modules.Storage.Application.Folders.BusinesRules;

namespace MyOS.UnitTests.Application.Storage.Folders.BusinessRules
{
    public class FolderMustNotCreateCycleRuleTests
    {
        [Fact]
        public async Task CheckAsync_MoveToRoot_Passes() =>
            (await new FolderMustNotCreateCycleRule(null, [Guid.NewGuid()])
                .CheckAsync(CancellationToken.None)).ShouldBeTrue();

        [Fact]
        public async Task CheckAsync_TargetInsideOwnSubtree_Fails()
        {
            var target = Guid.NewGuid();

            (await new FolderMustNotCreateCycleRule(target, [Guid.NewGuid(), target])
                .CheckAsync(CancellationToken.None)).ShouldBeFalse();
        }

        [Fact]
        public async Task CheckAsync_TargetOutsideSubtree_Passes() =>
            (await new FolderMustNotCreateCycleRule(Guid.NewGuid(), [Guid.NewGuid()])
                .CheckAsync(CancellationToken.None)).ShouldBeTrue();

        [Fact]
        public void Error_IsCircularReference() =>
            new FolderMustNotCreateCycleRule(null, []).Error.ShouldBe(FolderErrors.CircularReference);
    }
}
