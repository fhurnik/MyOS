namespace MyOS.Modules.Fitness.Domain.Exercises
{
    public interface IExerciseRepository
    {
        Task AddAsync(Exercise exercise, CancellationToken cancellationToken);

        // Soft-deleted rows are excluded via the EF global query filter (configured in Infrastructure).
        Task<Exercise?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    }
}
