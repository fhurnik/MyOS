using MyOS.Modules.Fitness.Domain.Workouts;

namespace MyOS.UnitTests.Domain.Fitness.Workouts
{
    // WorkoutExercise owns the set collection. Its decisions mirror the aggregate root's
    // exercise-level logic (sequential positions that skip soft-deleted rows, soft-delete-aware
    // lookup), but operate on sets — so they get their own coverage.
    public class WorkoutExerciseTests
    {
        // A strength entry is the only one that carries sets; obtain it through the aggregate root.
        private static WorkoutExercise NewStrengthEntry()
        {
            var workout = Workout.Create(Guid.NewGuid(), new DateOnly(2026, 6, 21), notes: null);
            return workout.AddStrengthExercise(Guid.NewGuid());
        }

        [Fact]
        public void AddWeightedSet_Twice_AssignsSequentialPositions()
        {
            var entry = NewStrengthEntry();

            var first = entry.AddWeightedSet(reps: 5, weight: 100m, rir: 2);
            var second = entry.AddWeightedSet(reps: 5, weight: 105m, rir: 1);

            first.Position.ShouldBe(1);
            second.Position.ShouldBe(2);
        }

        [Fact]
        public void AddSet_AfterRemovingFirst_ContinuesFromMaxPosition()
        {
            var entry = NewStrengthEntry();
            var a = entry.AddWeightedSet(reps: 5, weight: 100m, rir: null); // position 1
            entry.AddWeightedSet(reps: 5, weight: 100m, rir: null);         // position 2
            entry.RemoveSet(a.Id);                                          // soft-delete position 1

            var c = entry.AddBodyweightSet(reps: 8, addedWeight: null, negatives: null, rir: null);

            // NextSetPosition() takes the max of NON-deleted positions + 1 (here {2} -> 3);
            // freed slots are not reused and deleted rows are not counted.
            c.Position.ShouldBe(3);
        }

        [Fact]
        public void RemoveSet_UnknownId_ReturnsFalse()
        {
            var entry = NewStrengthEntry();
            entry.AddWeightedSet(reps: 5, weight: 100m, rir: null);

            entry.RemoveSet(Guid.NewGuid()).ShouldBeFalse();
        }

        [Fact]
        public void RemoveSet_ExistingId_SoftDeletesSoItIsNoLongerFoundOrCounted()
        {
            var entry = NewStrengthEntry();
            var set = entry.AddWeightedSet(reps: 5, weight: 100m, rir: null);

            var removed = entry.RemoveSet(set.Id);

            removed.ShouldBeTrue();
            entry.FindSet(set.Id).ShouldBeNull();   // FindSet ignores soft-deleted sets
            entry.ActiveSetCount.ShouldBe(0);       // and they drop out of the active count
        }
    }
}
