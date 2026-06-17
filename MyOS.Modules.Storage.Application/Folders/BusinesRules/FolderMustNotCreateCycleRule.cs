using MyOS.Core.Application.Abstractions.BusinessRules;
using MyOS.Core.Application.Abstractions.Results;
using MyOS.Modules.Storage.Application.Errors;

namespace MyOS.Modules.Storage.Application.Folders.BusinesRules
{
    // Moving a folder under itself or one of its descendants would create a cycle.
    // The handler supplies the moved folder's subtree ids; the rule only checks the target.
    internal sealed class FolderMustNotCreateCycleRule(
        Guid? targetParentId, IReadOnlyCollection<Guid> subtreeIds) : IBusinessRule
    {
        public Error Error => FolderErrors.CircularReference;

        public Task<bool> CheckAsync(CancellationToken cancellationToken) =>
            Task.FromResult(targetParentId is null || !subtreeIds.Contains(targetParentId.Value));
    }
}
