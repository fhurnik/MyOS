using Microsoft.EntityFrameworkCore;
using MyOS.Core.Infrastructure.EntityFrameworkConfiguration;
using MyOS.Modules.Fitness.Domain.Workouts;

namespace MyOS.Modules.Fitness.Infrastructure.Repositories
{
    internal sealed class WorkoutRepository(AppDbContext dbContext) : IWorkoutRepository
    {
        public async Task AddAsync(Workout workout, CancellationToken cancellationToken) =>
            await dbContext.Set<Workout>().AddAsync(workout, cancellationToken);

        // Add (not Attach/Update) marks the entity and every reachable untracked entity as
        // Added, regardless of their pre-set keys. This avoids the change-tracker's graph
        // cascade marking new children as Modified (which produces a 0-rows UPDATE).
        public void AddExercise(WorkoutExercise entry) =>
            dbContext.Set<WorkoutExercise>().Add(entry);

        public void AddSet(ExerciseSet set) =>
            dbContext.Set<ExerciseSet>().Add(set);

        public Task<Workout?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
            dbContext.Set<Workout>()
                .Include(w => w.Exercises)
                    .ThenInclude(e => e.Sets)
                .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);

        // Conservative: any non-deleted workout entry counts as "in use" (even under a soft-deleted
        // workout that could be restored), so the exercise's immutable fields stay locked.
        public Task<bool> ExistsByExerciseAsync(Guid exerciseId, CancellationToken cancellationToken) =>
            dbContext.Set<WorkoutExercise>()
                .AnyAsync(we => we.ExerciseId == exerciseId, cancellationToken);
    }
}
