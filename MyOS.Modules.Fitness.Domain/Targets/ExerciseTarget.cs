using MyOS.Core.Domain.Entities;

namespace MyOS.Modules.Fitness.Domain.Targets
{
    // One current target per exercise. Value is the same single metric as the progression
    // (weighted = weight, bodyweight = reps, cardio = duration). Replaced in place (no soft delete).
    public class ExerciseTarget : Entity
    {
        public Guid ExerciseId { get; private set; }
        public Guid UserId { get; private set; }
        public decimal Value { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }
        public DateTime? UpdatedAtUtc { get; private set; }

        public static ExerciseTarget Create(Guid exerciseId, Guid userId, decimal value) =>
            new(exerciseId, userId, value);

        internal ExerciseTarget(Guid exerciseId, Guid userId, decimal value)
        {
            Id = Guid.NewGuid();
            ExerciseId = exerciseId;
            UserId = userId;
            Value = value;
            CreatedAtUtc = DateTime.UtcNow;
        }

        private ExerciseTarget()
        {
            // for EF Core
        }

        internal void ChangeValue(decimal value)
        {
            Value = value;
            UpdatedAtUtc = DateTime.UtcNow;
        }
    }
}
