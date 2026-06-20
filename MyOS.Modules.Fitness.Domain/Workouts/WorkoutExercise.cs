using MyOS.Core.Domain.Entities;

namespace MyOS.Modules.Fitness.Domain.Workouts
{
    // A single exercise entry within a workout. Cardio carries Duration; strength carries sets.
    // Always mutated through the Workout aggregate root.
    public class WorkoutExercise : Entity
    {
        public Guid WorkoutId { get; private set; }
        public Guid ExerciseId { get; private set; }
        public int Position { get; private set; }
        public int? Duration { get; private set; } // cardio only, seconds
        public DateTime CreatedAtUtc { get; private set; }
        public DateTime? DeletedAtUtc { get; private set; }

        private readonly List<ExerciseSet> _sets = [];
        public IReadOnlyCollection<ExerciseSet> Sets => _sets.AsReadOnly();

        internal static WorkoutExercise CreateCardio(Guid workoutId, Guid exerciseId, int position, int duration)
        {
            var entry = new WorkoutExercise(workoutId, exerciseId, position);
            entry.Duration = duration;
            return entry;
        }

        internal static WorkoutExercise CreateStrength(Guid workoutId, Guid exerciseId, int position) =>
            new(workoutId, exerciseId, position);

        private WorkoutExercise(Guid workoutId, Guid exerciseId, int position)
        {
            Id = Guid.NewGuid();
            WorkoutId = workoutId;
            ExerciseId = exerciseId;
            Position = position;
            CreatedAtUtc = DateTime.UtcNow;
        }

        private WorkoutExercise()
        {
            // for EF Core
        }

        internal void ChangeDuration(int duration)
        {
            Duration = duration;
        }

        internal ExerciseSet AddWeightedSet(int reps, decimal weight, byte? rir)
        {
            var set = ExerciseSet.CreateWeighted(Id, NextSetPosition(), reps, weight, rir);
            _sets.Add(set);
            return set;
        }

        internal ExerciseSet AddBodyweightSet(int reps, decimal? addedWeight, int? negatives, byte? rir)
        {
            var set = ExerciseSet.CreateBodyweight(Id, NextSetPosition(), reps, addedWeight, negatives, rir);
            _sets.Add(set);
            return set;
        }

        internal ExerciseSet? FindSet(Guid setId) =>
            _sets.FirstOrDefault(s => s.Id == setId && s.DeletedAtUtc is null);

        internal bool RemoveSet(Guid setId)
        {
            var set = FindSet(setId);
            if (set is null)
                return false;

            set.Delete();
            return true;
        }

        internal void Delete()
        {
            DeletedAtUtc = DateTime.UtcNow;
        }

        private int NextSetPosition() =>
            _sets.Where(s => s.DeletedAtUtc is null)
                .Select(s => s.Position)
                .DefaultIfEmpty(0)
                .Max() + 1;
    }
}
