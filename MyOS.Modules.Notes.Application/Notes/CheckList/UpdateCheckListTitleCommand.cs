using FluentValidation;
using MediatR;
using MyOS.Core.Application.Abstractions;
using MyOS.Core.Application.Abstractions.Messaging;
using MyOS.Core.Application.Abstractions.Results;
using MyOS.Modules.Notes.Application.Errors;
using MyOS.Modules.Notes.Domain.Notes.CheckList;

namespace MyOS.Modules.Notes.Application.Notes.CheckList
{
    public sealed record UpdateCheckListTitleCommand(Guid Id, string Title) : ICommand<Unit>;

    public sealed class UpdateCheckListTitleCommandValidator : AbstractValidator<UpdateCheckListTitleCommand>
    {
        public UpdateCheckListTitleCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Title).NotEmpty().MaximumLength(500);
        }
    }

    internal sealed class UpdateCheckListTitleCommandHandler(
        ICheckListRepository checkListRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork) : ICommandHandler<UpdateCheckListTitleCommand, Unit>
    {
        public async Task<Result<Unit>> Handle(UpdateCheckListTitleCommand command, CancellationToken cancellationToken)
        {
            var checkList = await checkListRepository.GetByIdAsync(command.Id, cancellationToken);
            if (checkList is null)
                return Result<Unit>.Failure(CheckListErrors.NotFound);

            if (checkList.UserId != currentUser.Id)
                return Result<Unit>.Failure(CheckListErrors.Forbidden);

            checkList.UpdateTitle(command.Title);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Unit>.Success(Unit.Value);
        }
    }
}
