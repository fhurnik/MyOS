namespace MyOS.Modules.Fitness.Domain.Workouts
{
    public interface IWorkoutRepository
    {
        Task AddAsync(Workout workout, CancellationToken cancellationToken);

        // Loads the full aggregate graph (exercises + sets). Soft-deleted rows are excluded
        // by the global query filter. Mutate through the root, then SaveChanges once.
        Task<Workout?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

        // Explicitly tracks newly created children as Added. New entities carry pre-set keys
        // (Id is assigned in the constructor), so the change-tracker's graph cascade can mark
        // them Modified instead of Added — producing a 0-rows UPDATE. Adding them through the
        // context guarantees Added regardless of where they sit in the aggregate graph.
        // AddExercise also cascades to the entry's sets.
        void AddExercise(WorkoutExercise entry);

        void AddSet(ExerciseSet set);

        // True if the exercise is referenced by any (non-deleted) workout entry — drives the
        // exercise immutability rule.
        Task<bool> ExistsByExerciseAsync(Guid exerciseId, CancellationToken cancellationToken);
    }
}
