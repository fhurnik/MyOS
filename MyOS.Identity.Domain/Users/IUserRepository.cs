namespace MyOS.Identity.Domain.Users
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken);
        Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task AddAsync(User user, CancellationToken cancellationToken);
        Task<RefreshToken?> GetRefreshTokenAsync(string token, CancellationToken cancellationToken);
        Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken);
    }
}
