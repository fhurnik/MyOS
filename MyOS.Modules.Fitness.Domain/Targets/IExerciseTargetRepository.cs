namespace MyOS.Modules.Fitness.Domain.Targets
{
    public interface IExerciseTargetRepository
    {
        Task AddAsync(ExerciseTarget target, CancellationToken cancellationToken);

        Task<ExerciseTarget?> GetByExerciseIdAsync(Guid exerciseId, CancellationToken cancellationToken);
    }
}
