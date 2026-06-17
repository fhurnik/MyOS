using Microsoft.EntityFrameworkCore;
using MyOS.Core.Infrastructure.EntityFrameworkConfiguration;
using MyOS.Modules.Storage.Domain.AllowedFileTypes;

namespace MyOS.Modules.Storage.Infrastructure.Repositories
{
    internal sealed class AllowedFileTypeRepository(AppDbContext dbContext) : IAllowedFileTypeRepository
    {
        public Task<AllowedFileType?> GetByExtensionAsync(string extension, CancellationToken cancellationToken) =>
            dbContext.Set<AllowedFileType>()
                .FirstOrDefaultAsync(t => t.Extension == extension, cancellationToken);
    }
}
