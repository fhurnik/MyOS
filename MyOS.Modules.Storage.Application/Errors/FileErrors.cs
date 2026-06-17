using MyOS.Core.Application.Abstractions;
using MyOS.Core.Application.Abstractions.Results;

namespace MyOS.Modules.Storage.Application.Errors
{
    public sealed class FileErrors : ErrorCodes
    {
        private FileErrors() { } // reflection only — see ErrorCodes base class

        public static readonly Error NotFound =
            Error.NotFound("FileErrors.NotFound");

        public static readonly Error Forbidden =
            Error.Forbidden("FileErrors.Forbidden");

        public static readonly Error TypeNotAllowed =
            Error.Validation("FileErrors.TypeNotAllowed");

        public static readonly Error ContentMismatch =
            Error.Validation("FileErrors.ContentMismatch");

        public static readonly Error PhysicalFileMissing =
            Error.Failure("FileErrors.PhysicalFileMissing");

        public static readonly Error InlineNotAllowed =
            Error.Validation("FileErrors.InlineNotAllowed");
    }
}
