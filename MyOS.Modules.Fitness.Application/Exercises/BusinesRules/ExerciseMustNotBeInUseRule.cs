using MyOS.Core.Application.Abstractions.BusinessRules;
using MyOS.Core.Application.Abstractions.Results;
using MyOS.Modules.Fitness.Application.Errors;
using MyOS.Modules.Fitness.Domain.Workouts;

namespace MyOS.Modules.Fitness.Application.Exercises.BusinesRules
{
    // Inherently a cross-aggregate query (is this exercise referenced by any workout?), so the
    // rule injects the repository — the allowed exception from the "rules never load data" guideline.
    internal sealed class ExerciseMustNotBeInUseRule(
        IWorkoutRepository workoutRepository,
        Guid exerciseId) : IBusinessRule
    {
        public Error Error => ExerciseErrors.InUse;

        public async Task<bool> CheckAsync(CancellationToken cancellationToken) =>
            !await workoutRepository.ExistsByExerciseAsync(exerciseId, cancellationToken);
    }
}
