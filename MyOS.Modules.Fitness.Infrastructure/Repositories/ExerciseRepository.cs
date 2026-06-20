using Microsoft.EntityFrameworkCore;
using MyOS.Core.Infrastructure.EntityFrameworkConfiguration;
using MyOS.Modules.Fitness.Domain.Exercises;

namespace MyOS.Modules.Fitness.Infrastructure.Repositories
{
    internal sealed class ExerciseRepository(AppDbContext dbContext) : IExerciseRepository
    {
        public async Task AddAsync(Exercise exercise, CancellationToken cancellationToken) =>
            await dbContext.Set<Exercise>().AddAsync(exercise, cancellationToken);

        public Task<Exercise?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
            dbContext.Set<Exercise>()
                .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }
}
