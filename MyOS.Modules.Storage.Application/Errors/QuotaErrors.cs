using MyOS.Core.Application.Abstractions;
using MyOS.Core.Application.Abstractions.Results;

namespace MyOS.Modules.Storage.Application.Errors
{
    public sealed class QuotaErrors : ErrorCodes
    {
        private QuotaErrors() { } // reflection only — see ErrorCodes base class

        public static readonly Error NotFound =
            Error.NotFound("QuotaErrors.NotFound");

        public static readonly Error InsufficientSpace =
            Error.Conflict("QuotaErrors.InsufficientSpace");
    }
}
