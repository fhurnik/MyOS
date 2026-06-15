using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MyOS.Core.Application.Abstractions;
using MyOS.Core.Application.Exceptions;
using MyOS.Core.Infrastructure.EntityFrameworkConfiguration;

namespace MyOS.Core.Infrastructure.Persistence
{
    internal sealed class UnitOfWork(AppDbContext dbContext) : IUnitOfWork
    {
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            try
            {
                return await dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
            {
                throw new UniqueConstraintViolationException(ex);
            }
        }
    }
}
