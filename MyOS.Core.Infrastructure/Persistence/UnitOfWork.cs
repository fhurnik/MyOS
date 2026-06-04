using MyOS.Core.Application.Abstractions;
using MyOS.Core.Infrastructure.EntityFrameworkConfiguration;

namespace MyOS.Core.Infrastructure.Persistence
{
    internal sealed class UnitOfWork(AppDbContext dbContext) : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken) =>
            dbContext.SaveChangesAsync(cancellationToken);
    }
}
