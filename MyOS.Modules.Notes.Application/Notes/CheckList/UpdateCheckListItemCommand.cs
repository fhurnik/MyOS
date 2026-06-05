using FluentValidation;
using MediatR;
using MyOS.Core.Application.Abstractions;
using MyOS.Core.Application.Abstractions.Messaging;
using MyOS.Core.Application.Abstractions.Results;
using MyOS.Modules.Notes.Application.Errors;
using MyOS.Modules.Notes.Domain.Notes.CheckList;

namespace MyOS.Modules.Notes.Application.Notes.CheckList
{
    public sealed record UpdateCheckListItemCommand(Guid CheckListId, Guid ItemId, string Text) : ICommand<Unit>;

    public sealed class UpdateCheckListItemCommandValidator : AbstractValidator<UpdateCheckListItemCommand>
    {
        public UpdateCheckListItemCommandValidator()
        {
            RuleFor(x => x.CheckListId).NotEmpty();
            RuleFor(x => x.ItemId).NotEmpty();
            RuleFor(x => x.Text).NotEmpty().MaximumLength(2000);
        }
    }

    internal sealed class UpdateCheckListItemCommandHandler(
        ICheckListRepository checkListRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork) : ICommandHandler<UpdateCheckListItemCommand, Unit>
    {
        public async Task<Result<Unit>> Handle(UpdateCheckListItemCommand command, CancellationToken cancellationToken)
        {
            var checkList = await checkListRepository.GetByIdAsync(command.CheckListId, cancellationToken);
            if (checkList is null)
                return Result<Unit>.Failure(CheckListErrors.NotFound);

            if (checkList.UserId != currentUser.Id)
                return Result<Unit>.Failure(CheckListErrors.Forbidden);

            if (!checkList.UpdateItem(command.ItemId, command.Text))
                return Result<Unit>.Failure(CheckListErrors.ItemNotFound);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<Unit>.Success(Unit.Value);
        }
    }
}
