using FluentValidation;
using MediatR;
using MyOS.Core.Application.Abstractions;
using MyOS.Core.Application.Abstractions.Messaging;
using MyOS.Core.Application.Abstractions.Results;
using MyOS.Modules.Notes.Application.Errors;
using MyOS.Modules.Notes.Domain.Notes.TextNotes;

namespace MyOS.Modules.Notes.Application.Notes.TextNotes
{
    public sealed record UpdateTextNoteCommand(Guid Id, string Title, string Text) : ICommand<Unit>;

    public sealed class UpdateTextNoteCommandValidator : AbstractValidator<UpdateTextNoteCommand>
    {
        public UpdateTextNoteCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Title).NotEmpty().MaximumLength(500);
            RuleFor(x => x.Text).NotEmpty();
        }
    }

    internal sealed class UpdateTextNoteCommandHandler(
        ITextNoteRepository textNoteRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork) : ICommandHandler<UpdateTextNoteCommand, Unit>
    {
        public async Task<Result<Unit>> Handle(UpdateTextNoteCommand command, CancellationToken cancellationToken)
        {
            var note = await textNoteRepository.GetByIdAsync(command.Id, cancellationToken);
            if (note is null)
                return Result<Unit>.Failure(TextNoteErrors.NotFound);

            if (note.UserId != currentUser.Id)
                return Result<Unit>.Failure(TextNoteErrors.Forbidden);

            note.Update(command.Title, command.Text);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Unit>.Success(Unit.Value);
        }
    }
}
