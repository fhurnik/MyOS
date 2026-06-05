using FluentValidation;
using MyOS.Core.Application.Abstractions;
using MyOS.Core.Application.Abstractions.Messaging;
using MyOS.Core.Application.Abstractions.Results;
using DomainCheckList = MyOS.Modules.Notes.Domain.Notes.CheckList.CheckList;
using MyOS.Modules.Notes.Domain.Notes.CheckList;

namespace MyOS.Modules.Notes.Application.Notes.CheckList
{
    public sealed record CreateCheckListCommand(string Title) : ICommand<Guid>;

    public sealed class CreateCheckListCommandValidator : AbstractValidator<CreateCheckListCommand>
    {
        public CreateCheckListCommandValidator()
        {
            RuleFor(x => x.Title).NotEmpty().MaximumLength(500);
        }
    }

    internal sealed class CreateCheckListCommandHandler(
        ICheckListRepository checkListRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork) : ICommandHandler<CreateCheckListCommand, Guid>
    {
        public async Task<Result<Guid>> Handle(CreateCheckListCommand command, CancellationToken cancellationToken)
        {
            var checkList = DomainCheckList.Create(currentUser.Id, command.Title);

            await checkListRepository.AddAsync(checkList, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(checkList.Id);
        }
    }
}
