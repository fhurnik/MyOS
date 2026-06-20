using Microsoft.EntityFrameworkCore;
using MyOS.Core.Infrastructure.EntityFrameworkConfiguration;
using MyOS.Modules.Fitness.Domain.Targets;

namespace MyOS.Modules.Fitness.Infrastructure.Repositories
{
    internal sealed class ExerciseTargetRepository(AppDbContext dbContext) : IExerciseTargetRepository
    {
        public async Task AddAsync(ExerciseTarget target, CancellationToken cancellationToken) =>
            await dbContext.Set<ExerciseTarget>().AddAsync(target, cancellationToken);

        public Task<ExerciseTarget?> GetByExerciseIdAsync(Guid exerciseId, CancellationToken cancellationToken) =>
            dbContext.Set<ExerciseTarget>()
                .FirstOrDefaultAsync(t => t.ExerciseId == exerciseId, cancellationToken);
    }
}
