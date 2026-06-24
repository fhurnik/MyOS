using MyOS.Modules.Fitness.Application.Workouts;

namespace MyOS.UnitTests.Application.Fitness.Workouts
{
    // Validator test (kept because the date rule is real business validation, not framework noise).
    // Encodes the REQUIREMENT — a workout cannot be logged in the future — rather than the current
    // behavior. This is the "confront with reality" check: it goes red against buggy code.
    public class CreateWorkoutCommandValidatorTests
    {
        private readonly CreateWorkoutCommandValidator _validator = new();

        private static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);

        [Fact]
        public void Validate_DateInTheFuture_IsInvalid()
        {
            var result = _validator.Validate(new CreateWorkoutCommand(Today.AddDays(1), null));

            result.IsValid.ShouldBeFalse();
        }

        [Fact]
        public void Validate_DateToday_IsValid()
        {
            var result = _validator.Validate(new CreateWorkoutCommand(Today, null));

            result.IsValid.ShouldBeTrue();
        }

        [Fact]
        public void Validate_DateInThePast_IsValid()
        {
            var result = _validator.Validate(new CreateWorkoutCommand(Today.AddDays(-30), "back-filled"));

            result.IsValid.ShouldBeTrue();
        }
    }
}
