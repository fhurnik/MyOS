using MediatR;
using MyOS.Core.Application.Abstractions.Results;

namespace MyOS.Core.Application.Abstractions.Messaging
{
    public interface ICommand : IRequest<Result>;

    public interface ICommand<T> : IRequest<Result<T>>;
}
