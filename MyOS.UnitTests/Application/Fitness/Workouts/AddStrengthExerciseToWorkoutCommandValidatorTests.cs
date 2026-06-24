using MyOS.Modules.Fitness.Application.Workouts;
using MyOS.Modules.Fitness.Application.Workouts.Shared;

namespace MyOS.UnitTests.Application.Fitness.Workouts
{
    // Requirement: a strength exercise must be logged with at least one set.
    public class AddStrengthExerciseToWorkoutCommandValidatorTests
    {
        private readonly AddStrengthExerciseToWorkoutCommandValidator _validator = new();

        [Fact]
        public void Validate_NoSets_IsInvalid()
        {
            var result = _validator.Validate(
                new AddStrengthExerciseToWorkoutCommand(Guid.NewGuid(), Guid.NewGuid(), []));

            result.IsValid.ShouldBeFalse();
        }

        [Fact]
        public void Validate_WithAtLeastOneValidSet_IsValid()
        {
            IReadOnlyList<SetInput> sets =
                [new SetInput(Reps: 8, Weight: 100m, AddedWeight: null, Negatives: null, Rir: 2)];

            var result = _validator.Validate(
                new AddStrengthExerciseToWorkoutCommand(Guid.NewGuid(), Guid.NewGuid(), sets));

            result.IsValid.ShouldBeTrue();
        }

        [Fact]
        public void Validate_WeightedSetWithNegativeWeight_IsInvalid()
        {
            IReadOnlyList<SetInput> sets =
                [new SetInput(Reps: 8, Weight: -50m, AddedWeight: null, Negatives: null, Rir: null)];

            var result = _validator.Validate(
                new AddStrengthExerciseToWorkoutCommand(Guid.NewGuid(), Guid.NewGuid(), sets));

            result.IsValid.ShouldBeFalse();
        }

        [Fact]
        public void Validate_BodyweightSetWithNegativeAddedWeight_IsInvalid()
        {
            IReadOnlyList<SetInput> sets =
                [new SetInput(Reps: 8, Weight: null, AddedWeight: -10m, Negatives: null, Rir: null)];

            var result = _validator.Validate(
                new AddStrengthExerciseToWorkoutCommand(Guid.NewGuid(), Guid.NewGuid(), sets));

            result.IsValid.ShouldBeFalse();
        }
    }
}
