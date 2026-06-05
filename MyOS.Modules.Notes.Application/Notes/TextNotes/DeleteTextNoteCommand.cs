using FluentValidation;
using MediatR;
using MyOS.Core.Application.Abstractions;
using MyOS.Core.Application.Abstractions.Messaging;
using MyOS.Core.Application.Abstractions.Results;
using MyOS.Modules.Notes.Application.Errors;
using MyOS.Modules.Notes.Domain.Notes.TextNotes;

namespace MyOS.Modules.Notes.Application.Notes.TextNotes
{
    public sealed record DeleteTextNoteCommand(Guid Id) : ICommand<Unit>;

    public sealed class DeleteTextNoteCommandValidator : AbstractValidator<DeleteTextNoteCommand>
    {
        public DeleteTextNoteCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
        }
    }

    internal sealed class DeleteTextNoteCommandHandler(
        ITextNoteRepository textNoteRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork) : ICommandHandler<DeleteTextNoteCommand, Unit>
    {
        public async Task<Result<Unit>> Handle(DeleteTextNoteCommand command, CancellationToken cancellationToken)
        {
            var note = await textNoteRepository.GetByIdAsync(command.Id, cancellationToken);
            if (note is null)
                return Result<Unit>.Failure(TextNoteErrors.NotFound);

            if (note.UserId != currentUser.Id)
                return Result<Unit>.Failure(TextNoteErrors.Forbidden);

            note.Delete();
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Unit>.Success(Unit.Value);
        }
    }
}
