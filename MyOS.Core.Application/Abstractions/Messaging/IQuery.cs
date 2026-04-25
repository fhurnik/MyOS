using MediatR;
using MyOS.Core.Application.Abstractions.Results;

namespace MyOS.Core.Application.Abstractions.Messaging
{
    public interface IQuery<TResponse> : IRequest<Result<TResponse>>;
}
