using MyOS.Core.Application.Abstractions;
using MyOS.Core.Application.Abstractions.Results;

namespace MyOS.Core.Application.Errors
{
    public sealed class PagingErrors : ErrorCodes
    {
        private PagingErrors() { } // reflection only — see ErrorCodes base class

        public static readonly Error InvalidOrderBy =
            Error.Validation("PagingErrors.InvalidOrderBy");
    }
}
