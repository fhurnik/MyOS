namespace MyOS.Modules.Fitness.Domain.Workouts
{
    public interface IWorkoutRepository
    {
        Task AddAsync(Workout workout, CancellationToken cancellationToken);

        // Loads the full aggregate graph (exercises + sets). Soft-deleted rows are excluded
        // by the global query filter. Mutate through the root, then SaveChanges once.
        Task<Workout?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

        // Tracks a newly created exercise entry (and its sets) for insert. Needed when an entry
        // is added together with its sets in one SaveChanges: the grandchild sets carry pre-set
        // keys, so the change-tracker's graph cascade would otherwise mark them Modified instead
        // of Added. Explicit Add forces the whole subgraph to Added.
        void AddExercise(WorkoutExercise entry);

        // True if the exercise is referenced by any (non-deleted) workout entry — drives the
        // exercise immutability rule.
        Task<bool> ExistsByExerciseAsync(Guid exerciseId, CancellationToken cancellationToken);
    }
}
