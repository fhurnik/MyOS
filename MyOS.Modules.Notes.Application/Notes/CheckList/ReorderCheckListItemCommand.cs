using FluentValidation;
using MediatR;
using MyOS.Core.Application.Abstractions;
using MyOS.Core.Application.Abstractions.Messaging;
using MyOS.Core.Application.Abstractions.Results;
using MyOS.Modules.Notes.Application.Errors;
using MyOS.Modules.Notes.Domain.Notes.CheckList;

namespace MyOS.Modules.Notes.Application.Notes.CheckList
{
    public sealed record ReorderCheckListItemCommand(Guid CheckListId, Guid ItemId, int NewOrder) : ICommand<Unit>;

    public sealed class ReorderCheckListItemCommandValidator : AbstractValidator<ReorderCheckListItemCommand>
    {
        public ReorderCheckListItemCommandValidator()
        {
            RuleFor(x => x.CheckListId).NotEmpty();
            RuleFor(x => x.ItemId).NotEmpty();
            RuleFor(x => x.NewOrder).GreaterThan(0);
        }
    }

    internal sealed class ReorderCheckListItemCommandHandler(
        ICheckListRepository checkListRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork) : ICommandHandler<ReorderCheckListItemCommand, Unit>
    {
        public async Task<Result<Unit>> Handle(ReorderCheckListItemCommand command, CancellationToken cancellationToken)
        {
            var checkList = await checkListRepository.GetByIdAsync(command.CheckListId, cancellationToken);
            if (checkList is null)
                return Result<Unit>.Failure(CheckListErrors.NotFound);

            if (checkList.UserId != currentUser.Id)
                return Result<Unit>.Failure(CheckListErrors.Forbidden);

            if (!checkList.ReorderItem(command.ItemId, command.NewOrder))
                return Result<Unit>.Failure(CheckListErrors.ItemNotFound);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<Unit>.Success(Unit.Value);
        }
    }
}
