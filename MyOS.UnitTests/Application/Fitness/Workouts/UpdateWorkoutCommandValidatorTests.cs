using MyOS.Modules.Fitness.Application.Workouts;

namespace MyOS.UnitTests.Application.Fitness.Workouts
{
    public class UpdateWorkoutCommandValidatorTests
    {
        private readonly UpdateWorkoutCommandValidator _validator = new();

        private static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);

        [Fact]
        public void Validate_DateInTheFuture_IsInvalid()
        {
            var result = _validator.Validate(
                new UpdateWorkoutCommand(Guid.NewGuid(), Today.AddDays(1), null));

            result.IsValid.ShouldBeFalse();
        }

        [Fact]
        public void Validate_DateTodayOrPast_IsValid()
        {
            _validator.Validate(new UpdateWorkoutCommand(Guid.NewGuid(), Today, null))
                .IsValid.ShouldBeTrue();
            _validator.Validate(new UpdateWorkoutCommand(Guid.NewGuid(), Today.AddDays(-7), null))
                .IsValid.ShouldBeTrue();
        }
    }
}
