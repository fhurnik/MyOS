using FluentValidation;
using MyOS.Core.Application.Abstractions;
using MyOS.Core.Application.Abstractions.Messaging;
using MyOS.Core.Application.Abstractions.Results;
using MyOS.Modules.Notes.Application.Errors;
using MyOS.Modules.Notes.Domain.Notes.CheckList;

namespace MyOS.Modules.Notes.Application.Notes.CheckList
{
    public sealed record AddCheckListItemCommand(Guid CheckListId, string Text) : ICommand<Guid>;

    public sealed class AddCheckListItemCommandValidator : AbstractValidator<AddCheckListItemCommand>
    {
        public AddCheckListItemCommandValidator()
        {
            RuleFor(x => x.CheckListId).NotEmpty();
            RuleFor(x => x.Text).NotEmpty().MaximumLength(2000);
        }
    }

    internal sealed class AddCheckListItemCommandHandler(
        ICheckListRepository checkListRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork) : ICommandHandler<AddCheckListItemCommand, Guid>
    {
        public async Task<Result<Guid>> Handle(AddCheckListItemCommand command, CancellationToken cancellationToken)
        {
            var checkList = await checkListRepository.GetByIdAsync(command.CheckListId, cancellationToken);
            if (checkList is null)
                return Result<Guid>.Failure(CheckListErrors.NotFound);

            if (checkList.UserId != currentUser.Id)
                return Result<Guid>.Failure(CheckListErrors.Forbidden);

            var item = checkList.AddItem(command.Text);
            await checkListRepository.AddItemAsync(item, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(item.Id);
        }
    }
}
