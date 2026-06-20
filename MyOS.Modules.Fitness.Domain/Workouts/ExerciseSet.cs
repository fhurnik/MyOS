using MyOS.Core.Domain.Entities;

namespace MyOS.Modules.Fitness.Domain.Workouts
{
    // A single logged set. Weighted: Reps + Weight. Bodyweight: Reps + AddedWeight + Negatives.
    // Correct shape is guaranteed by the CreateWeighted / CreateBodyweight factories.
    public class ExerciseSet : Entity
    {
        public Guid WorkoutExerciseId { get; private set; }
        public int Position { get; private set; }
        public int Reps { get; private set; }
        public decimal? Weight { get; private set; }       // weighted
        public decimal? AddedWeight { get; private set; }  // bodyweight
        public int? Negatives { get; private set; }        // bodyweight
        public byte? Rir { get; private set; }             // 0-10, lower = closer to failure
        public DateTime CreatedAtUtc { get; private set; }
        public DateTime? DeletedAtUtc { get; private set; }

        internal static ExerciseSet CreateWeighted(Guid workoutExerciseId, int position, int reps, decimal weight, byte? rir)
        {
            var set = new ExerciseSet(workoutExerciseId, position, reps);
            set.Weight = weight;
            set.Rir = rir;
            return set;
        }

        internal static ExerciseSet CreateBodyweight(Guid workoutExerciseId, int position, int reps, decimal? addedWeight, int? negatives, byte? rir)
        {
            var set = new ExerciseSet(workoutExerciseId, position, reps);
            set.AddedWeight = addedWeight;
            set.Negatives = negatives;
            set.Rir = rir;
            return set;
        }

        private ExerciseSet(Guid workoutExerciseId, int position, int reps)
        {
            Id = Guid.NewGuid();
            WorkoutExerciseId = workoutExerciseId;
            Position = position;
            Reps = reps;
            CreatedAtUtc = DateTime.UtcNow;
        }

        internal void UpdateWeighted(int reps, decimal weight, byte? rir)
        {
            Reps = reps;
            Weight = weight;
            Rir = rir;
        }

        internal void UpdateBodyweight(int reps, decimal? addedWeight, int? negatives, byte? rir)
        {
            Reps = reps;
            AddedWeight = addedWeight;
            Negatives = negatives;
            Rir = rir;
        }

        internal void Delete()
        {
            DeletedAtUtc = DateTime.UtcNow;
        }

        private ExerciseSet()
        {
            // for EF Core
        }
    }
}
