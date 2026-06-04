using MyOS.Core.Application.Abstractions.Results;

namespace MyOS.Identity.Application.Errors
{
    public static class UserErrors
    {
        public static readonly Error EmailAlreadyInUse =
            Error.Conflict("User.EmailAlreadyInUse", "A user with this email is already registered.");

        public static readonly Error InvalidCredentials =
            Error.Unauthorized("Auth.InvalidCredentials", "Email or password is incorrect.");

        public static readonly Error AccountDisabled =
            Error.Unauthorized("Auth.AccountDisabled", "Account is disabled.");

        public static readonly Error InvalidRefreshToken =
            Error.Unauthorized("Auth.InvalidRefreshToken", "Refresh token is invalid or expired.");

        public static readonly Error Unauthorized =
            Error.Unauthorized("Auth.Unauthorized", "Access token is missing or invalid.");

        public static readonly Error Forbidden =
            Error.Forbidden("Auth.Forbidden", "You do not have permission to access this resource.");
    }
}
