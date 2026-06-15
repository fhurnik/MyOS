using MyOS.Core.Application.Abstractions.Results;

namespace MyOS.Core.Application.Abstractions.BusinessRules
{
    public interface IBusinessRule
    {
        Error Error { get; }

        Task<bool> CheckAsync(CancellationToken cancellationToken);
    }
}
