using MediatR;
using MyOS.Core.Application.Abstractions.Results;

namespace MyOS.Core.Application.Abstractions.Messaging
{
    public interface IQueryHandler<Query, TResponse> : IRequestHandler<Query, Result<TResponse>> where Query : IQuery<TResponse>;
}
