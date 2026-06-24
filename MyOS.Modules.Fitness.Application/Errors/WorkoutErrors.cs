using MyOS.Core.Application.Abstractions;
using MyOS.Core.Application.Abstractions.Results;

namespace MyOS.Modules.Fitness.Application.Errors
{
    public sealed class WorkoutErrors : ErrorCodes
    {
        private WorkoutErrors() { } // reflection only — see ErrorCodes base class

        public static readonly Error NotFound =
            Error.NotFound("WorkoutErrors.NotFound");

        public static readonly Error Forbidden =
            Error.Forbidden("WorkoutErrors.Forbidden");

        public static readonly Error ExerciseNotFound =
            Error.Validation("WorkoutErrors.ExerciseNotFound");

        public static readonly Error WorkoutExerciseNotFound =
            Error.NotFound("WorkoutErrors.WorkoutExerciseNotFound");

        public static readonly Error SetNotFound =
            Error.NotFound("WorkoutErrors.SetNotFound");

        public static readonly Error ActivityTypeMismatch =
            Error.Validation("WorkoutErrors.ActivityTypeMismatch");

        public static readonly Error ExerciseAlreadyInWorkout =
            Error.Conflict("WorkoutErrors.ExerciseAlreadyInWorkout");

        public static readonly Error LastSetCannotBeRemoved =
            Error.Conflict("WorkoutErrors.LastSetCannotBeRemoved");
    }
}
