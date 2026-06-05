using FluentValidation;
using MyOS.Core.Application.Abstractions;
using MyOS.Core.Application.Abstractions.Messaging;
using MyOS.Core.Application.Abstractions.Results;
using MyOS.Modules.Notes.Domain.Notes.TextNotes;

namespace MyOS.Modules.Notes.Application.Notes.TextNotes
{
    public sealed record CreateTextNoteCommand(string Title, string Text) : ICommand<Guid>;

    public sealed class CreateTextNoteCommandValidator : AbstractValidator<CreateTextNoteCommand>
    {
        public CreateTextNoteCommandValidator()
        {
            RuleFor(x => x.Title).NotEmpty().MaximumLength(500);
            RuleFor(x => x.Text).NotEmpty();
        }
    }

    internal sealed class CreateTextNoteCommandHandler(
        ITextNoteRepository textNoteRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork) : ICommandHandler<CreateTextNoteCommand, Guid>
    {
        public async Task<Result<Guid>> Handle(CreateTextNoteCommand command, CancellationToken cancellationToken)
        {
            var note = TextNote.Create(currentUser.Id, command.Title, command.Text);

            await textNoteRepository.AddAsync(note, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(note.Id);
        }
    }
}
