namespace MyOS.Core.Application.Abstractions
{
    public interface ICurrentUser
    {
        Guid Id { get; }
        string Email { get; }
        bool IsAuthenticated { get; }
    }
}
