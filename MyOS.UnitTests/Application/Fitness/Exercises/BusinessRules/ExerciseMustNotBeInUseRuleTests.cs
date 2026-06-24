using MyOS.Modules.Fitness.Application.Errors;
using MyOS.Modules.Fitness.Application.Exercises.BusinesRules;
using MyOS.Modules.Fitness.Domain.Workouts;

namespace MyOS.UnitTests.Application.Fitness.Exercises.BusinessRules
{
    // Business rules are tested as a pass/fail pair plus the error they carry.
    public class ExerciseMustNotBeInUseRuleTests
    {
        private readonly IWorkoutRepository _workouts = Substitute.For<IWorkoutRepository>();

        [Fact]
        public async Task CheckAsync_ExerciseNotReferencedByAnyWorkout_Passes()
        {
            var exerciseId = Guid.NewGuid();
            _workouts.ExistsByExerciseAsync(exerciseId, Arg.Any<CancellationToken>()).Returns(false);

            var rule = new ExerciseMustNotBeInUseRule(_workouts, exerciseId);

            (await rule.CheckAsync(CancellationToken.None)).ShouldBeTrue();
        }

        [Fact]
        public async Task CheckAsync_ExerciseReferencedByAWorkout_Fails()
        {
            var exerciseId = Guid.NewGuid();
            _workouts.ExistsByExerciseAsync(exerciseId, Arg.Any<CancellationToken>()).Returns(true);

            var rule = new ExerciseMustNotBeInUseRule(_workouts, exerciseId);

            (await rule.CheckAsync(CancellationToken.None)).ShouldBeFalse();
        }

        [Fact]
        public void Error_IsInUse()
        {
            var rule = new ExerciseMustNotBeInUseRule(_workouts, Guid.NewGuid());

            rule.Error.ShouldBe(ExerciseErrors.InUse);
        }
    }
}
