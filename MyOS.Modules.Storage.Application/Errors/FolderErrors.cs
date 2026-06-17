using MyOS.Core.Application.Abstractions;
using MyOS.Core.Application.Abstractions.Results;

namespace MyOS.Modules.Storage.Application.Errors
{
    public sealed class FolderErrors : ErrorCodes
    {
        private FolderErrors() { } // reflection only — see ErrorCodes base class

        public static readonly Error NotFound =
            Error.NotFound("FolderErrors.NotFound");

        public static readonly Error Forbidden =
            Error.Forbidden("FolderErrors.Forbidden");

        public static readonly Error ParentNotFound =
            Error.Validation("FolderErrors.ParentNotFound");

        public static readonly Error CircularReference =
            Error.Validation("FolderErrors.CircularReference");
    }
}
