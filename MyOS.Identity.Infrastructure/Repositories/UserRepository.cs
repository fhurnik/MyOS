using Microsoft.EntityFrameworkCore;
using MyOS.Core.Infrastructure.EntityFrameworkConfiguration;
using MyOS.Identity.Domain.Users;

namespace MyOS.Identity.Infrastructure.Repositories
{
    internal sealed class UserRepository(AppDbContext dbContext) : IUserRepository
    {
        public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken) =>
            dbContext.Set<User>()
                .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
            dbContext.Set<User>()
                .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        public async Task AddAsync(User user, CancellationToken cancellationToken) =>
            await dbContext.Set<User>().AddAsync(user, cancellationToken);

        public Task<RefreshToken?> GetRefreshTokenAsync(string token, CancellationToken cancellationToken) =>
            dbContext.Set<RefreshToken>()
                .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);

        public async Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken) =>
            await dbContext.Set<RefreshToken>().AddAsync(refreshToken, cancellationToken);
    }
}
