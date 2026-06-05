using FluentValidation;
using MediatR;
using MyOS.Core.Application.Abstractions;
using MyOS.Core.Application.Abstractions.Messaging;
using MyOS.Core.Application.Abstractions.Results;
using MyOS.Modules.Notes.Application.Errors;
using MyOS.Modules.Notes.Domain.Notes.CheckList;

namespace MyOS.Modules.Notes.Application.Notes.CheckList
{
    public sealed record ToggleCheckListItemCommand(Guid CheckListId, Guid ItemId) : ICommand<Unit>;

    public sealed class ToggleCheckListItemCommandValidator : AbstractValidator<ToggleCheckListItemCommand>
    {
        public ToggleCheckListItemCommandValidator()
        {
            RuleFor(x => x.CheckListId).NotEmpty();
            RuleFor(x => x.ItemId).NotEmpty();
        }
    }

    internal sealed class ToggleCheckListItemCommandHandler(
        ICheckListRepository checkListRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork) : ICommandHandler<ToggleCheckListItemCommand, Unit>
    {
        public async Task<Result<Unit>> Handle(ToggleCheckListItemCommand command, CancellationToken cancellationToken)
        {
            var checkList = await checkListRepository.GetByIdAsync(command.CheckListId, cancellationToken);
            if (checkList is null)
                return Result<Unit>.Failure(CheckListErrors.NotFound);

            if (checkList.UserId != currentUser.Id)
                return Result<Unit>.Failure(CheckListErrors.Forbidden);

            if (!checkList.ToggleItem(command.ItemId))
                return Result<Unit>.Failure(CheckListErrors.ItemNotFound);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<Unit>.Success(Unit.Value);
        }
    }
}
