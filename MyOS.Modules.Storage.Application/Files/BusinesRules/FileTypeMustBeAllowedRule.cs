using MyOS.Core.Application.Abstractions.BusinessRules;
using MyOS.Core.Application.Abstractions.Results;
using MyOS.Modules.Storage.Application.Errors;
using MyOS.Modules.Storage.Domain.AllowedFileTypes;

namespace MyOS.Modules.Storage.Application.Files.BusinesRules
{
    /// <summary>
    /// Passes when the file's extension maps to a configured, active allowed type.
    /// The handler fetches the type; this rule only validates its state.
    /// </summary>
    internal sealed class FileTypeMustBeAllowedRule(AllowedFileType? allowedType) : IBusinessRule
    {
        public Error Error => FileErrors.TypeNotAllowed;

        public Task<bool> CheckAsync(CancellationToken cancellationToken) =>
            Task.FromResult(allowedType is { IsActive: true });
    }
}
