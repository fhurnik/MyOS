using MyOS.Core.Domain.Entities;

namespace MyOS.Modules.Fitness.Domain.Workouts
{
    // Aggregate root. WorkoutExercise and ExerciseSet are mutated only through this root.
    public class Workout : Entity
    {
        public Guid UserId { get; private set; }
        public DateOnly Date { get; private set; }
        public string? Notes { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }
        public DateTime? UpdatedAtUtc { get; private set; }
        public DateTime? DeletedAtUtc { get; private set; }

        private readonly List<WorkoutExercise> _exercises = [];
        public IReadOnlyCollection<WorkoutExercise> Exercises => _exercises.AsReadOnly();

        public static Workout Create(Guid userId, DateOnly date, string? notes) =>
            new(userId, date, notes);

        internal Workout(Guid userId, DateOnly date, string? notes)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            Date = date;
            Notes = notes;
            CreatedAtUtc = DateTime.UtcNow;
        }

        private Workout()
        {
            // for EF Core
        }

        internal void Update(DateOnly date, string? notes)
        {
            Date = date;
            Notes = notes;
            Touch();
        }

        internal void Delete()
        {
            DeletedAtUtc = DateTime.UtcNow;
        }

        internal WorkoutExercise AddCardioExercise(Guid exerciseId, int duration)
        {
            var entry = WorkoutExercise.CreateCardio(Id, exerciseId, NextPosition(), duration);
            _exercises.Add(entry);
            Touch();
            return entry;
        }

        internal WorkoutExercise AddStrengthExercise(Guid exerciseId)
        {
            var entry = WorkoutExercise.CreateStrength(Id, exerciseId, NextPosition());
            _exercises.Add(entry);
            Touch();
            return entry;
        }

        internal WorkoutExercise? FindExercise(Guid workoutExerciseId) =>
            _exercises.FirstOrDefault(e => e.Id == workoutExerciseId && e.DeletedAtUtc is null);

        internal bool RemoveExercise(Guid workoutExerciseId)
        {
            var entry = FindExercise(workoutExerciseId);
            if (entry is null)
                return false;

            entry.Delete();
            Touch();
            return true;
        }

        private int NextPosition() =>
            _exercises.Where(e => e.DeletedAtUtc is null)
                .Select(e => e.Position)
                .DefaultIfEmpty(0)
                .Max() + 1;

        private void Touch()
        {
            UpdatedAtUtc = DateTime.UtcNow;
        }
    }
}
