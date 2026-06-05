using MyOS.Core.Application.Abstractions;
using MyOS.Core.Application.Abstractions.Results;

namespace MyOS.Modules.Notes.Application.Errors
{
    public sealed class CheckListErrors : ErrorCodes
    {
        private CheckListErrors() { } // reflection only — see ErrorCodes base class

        public static readonly Error NotFound =
            Error.NotFound("CheckListErrors.NotFound");

        public static readonly Error Forbidden =
            Error.Forbidden("CheckListErrors.Forbidden");

        public static readonly Error ItemNotFound =
            Error.NotFound("CheckListErrors.ItemNotFound");
    }
}
