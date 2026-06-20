namespace MyOS.Modules.Fitness.Domain.Workouts
{
    public interface IWorkoutRepository
    {
        Task AddAsync(Workout workout, CancellationToken cancellationToken);

        // Loads the full aggregate graph (exercises + sets). Soft-deleted rows are excluded
        // by the global query filter. Mutate through the root, then SaveChanges once.
        Task<Workout?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

        // True if the exercise is referenced by any (non-deleted) workout entry — drives the
        // exercise immutability rule.
        Task<bool> ExistsByExerciseAsync(Guid exerciseId, CancellationToken cancellationToken);
    }
}
