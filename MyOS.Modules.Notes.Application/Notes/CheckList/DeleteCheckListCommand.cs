using FluentValidation;
using MediatR;
using MyOS.Core.Application.Abstractions;
using MyOS.Core.Application.Abstractions.Messaging;
using MyOS.Core.Application.Abstractions.Results;
using MyOS.Modules.Notes.Application.Errors;
using MyOS.Modules.Notes.Domain.Notes.CheckList;

namespace MyOS.Modules.Notes.Application.Notes.CheckList
{
    public sealed record DeleteCheckListCommand(Guid Id) : ICommand<Unit>;

    public sealed class DeleteCheckListCommandValidator : AbstractValidator<DeleteCheckListCommand>
    {
        public DeleteCheckListCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
        }
    }

    internal sealed class DeleteCheckListCommandHandler(
        ICheckListRepository checkListRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork) : ICommandHandler<DeleteCheckListCommand, Unit>
    {
        public async Task<Result<Unit>> Handle(DeleteCheckListCommand command, CancellationToken cancellationToken)
        {
            var checkList = await checkListRepository.GetByIdAsync(command.Id, cancellationToken);
            if (checkList is null)
                return Result<Unit>.Failure(CheckListErrors.NotFound);

            if (checkList.UserId != currentUser.Id)
                return Result<Unit>.Failure(CheckListErrors.Forbidden);

            checkList.Delete();
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Unit>.Success(Unit.Value);
        }
    }
}
