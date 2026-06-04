using MyOS.Core.Application.Abstractions;
using MyOS.Core.Application.Abstractions.Results;

namespace MyOS.Identity.Application.Errors
{
    public sealed class UserErrors : ErrorCodes
    {
        private UserErrors() { } // reflection only — see ErrorCodes base class

        public static readonly Error EmailAlreadyInUse =
            Error.Conflict("UserErrors.EmailAlreadyInUse");

        public static readonly Error InvalidCredentials =
            Error.Unauthorized("UserErrors.InvalidCredentials");

        public static readonly Error AccountDisabled =
            Error.Unauthorized("UserErrors.AccountDisabled");

        public static readonly Error InvalidRefreshToken =
            Error.Unauthorized("UserErrors.InvalidRefreshToken");

        public static readonly Error Unauthorized =
            Error.Unauthorized("UserErrors.Unauthorized");

        public static readonly Error Forbidden =
            Error.Forbidden("UserErrors.Forbidden");
    }
}
