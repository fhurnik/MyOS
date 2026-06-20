using MyOS.Core.Application.Abstractions;
using MyOS.Core.Application.Abstractions.Results;

namespace MyOS.Modules.Fitness.Application.Errors
{
    public sealed class ExerciseErrors : ErrorCodes
    {
        private ExerciseErrors() { } // reflection only — see ErrorCodes base class

        public static readonly Error NotFound =
            Error.NotFound("ExerciseErrors.NotFound");

        public static readonly Error Forbidden =
            Error.Forbidden("ExerciseErrors.Forbidden");

        public static readonly Error ActivityTypeMismatch =
            Error.Validation("ExerciseErrors.ActivityTypeMismatch");

        public static readonly Error InUse =
            Error.Conflict("ExerciseErrors.InUse");
    }
}
