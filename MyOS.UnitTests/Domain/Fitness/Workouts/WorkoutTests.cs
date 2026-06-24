using MyOS.Modules.Fitness.Domain.Workouts;

namespace MyOS.UnitTests.Domain.Fitness.Workouts
{
    // Reference example of a DOMAIN unit test: pure, no mocks, no infrastructure.
    // Targets the decisions inside the aggregate (position assignment, soft-delete awareness).
    public class WorkoutTests
    {
        private static Workout NewWorkout() =>
            Workout.Create(userId: Guid.NewGuid(), date: new DateOnly(2026, 6, 21), notes: null);

        [Fact]
        public void AddStrengthExercise_Twice_AssignsSequentialPositions()
        {
            var workout = NewWorkout();

            var first = workout.AddStrengthExercise(Guid.NewGuid());
            var second = workout.AddStrengthExercise(Guid.NewGuid());

            first.Position.ShouldBe(1);
            second.Position.ShouldBe(2);
        }

        [Fact]
        public void AddStrengthExercise_AfterRemovingFirst_ContinuesFromMaxPosition()
        {
            var workout = NewWorkout();
            var a = workout.AddStrengthExercise(Guid.NewGuid()); // position 1
            workout.AddStrengthExercise(Guid.NewGuid());         // position 2
            workout.RemoveExercise(a.Id);                        // soft-delete position 1

            var c = workout.AddStrengthExercise(Guid.NewGuid());

            // NextPosition() computes max of NON-deleted positions + 1 (here {2} -> 3),
            // it does not reuse the freed slot and does not count deleted rows.
            c.Position.ShouldBe(3);
        }

        [Fact]
        public void RemoveExercise_UnknownId_ReturnsFalse()
        {
            var workout = NewWorkout();
            workout.AddStrengthExercise(Guid.NewGuid());

            var removed = workout.RemoveExercise(Guid.NewGuid());

            removed.ShouldBeFalse();
        }

        [Fact]
        public void RemoveExercise_ExistingId_SoftDeletesSoItIsNoLongerFound()
        {
            var workout = NewWorkout();
            var entry = workout.AddStrengthExercise(Guid.NewGuid());

            var removed = workout.RemoveExercise(entry.Id);

            removed.ShouldBeTrue();
            workout.FindExercise(entry.Id).ShouldBeNull(); // FindExercise ignores soft-deleted entries
        }

        [Fact]
        public void ContainsExercise_AfterSoftDelete_ReturnsFalseSoTheExerciseCanBeReadded()
        {
            var workout = NewWorkout();
            var exerciseId = Guid.NewGuid();
            var entry = workout.AddStrengthExercise(exerciseId);

            workout.ContainsExercise(exerciseId).ShouldBeTrue();

            workout.RemoveExercise(entry.Id);

            // The duplicate-guard used by the add-exercise handler must not see soft-deleted entries.
            workout.ContainsExercise(exerciseId).ShouldBeFalse();
        }
    }
}
