using MyOS.Core.Application.Abstractions;
using MyOS.Modules.Notes.Application.Errors;
using MyOS.Modules.Notes.Application.Notes.TextNotes;
using MyOS.Modules.Notes.Domain.Notes.TextNotes;
using MyOS.UnitTests.Common;

namespace MyOS.UnitTests.Application.Notes.TextNotes
{
    public class DeleteTextNoteCommandHandlerTests
    {
        private readonly ITextNoteRepository _notes = Substitute.For<ITextNoteRepository>();
        private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
        private readonly FakeCurrentUser _currentUser = new();

        private DeleteTextNoteCommandHandler CreateHandler() =>
            new(_notes, _currentUser, _unitOfWork);

        [Fact]
        public async Task Handle_NoteDoesNotExist_ReturnsNotFoundAndDoesNotSave()
        {
            _notes.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((TextNote?)null);

            var result = await CreateHandler().Handle(new DeleteTextNoteCommand(Guid.NewGuid()), CancellationToken.None);

            result.Error.ShouldBe(TextNoteErrors.NotFound);
            await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_NoteOwnedByAnotherUser_ReturnsForbiddenAndDoesNotSave()
        {
            var othersNote = TextNote.Create(Guid.NewGuid(), "T", "B");
            _notes.GetByIdAsync(othersNote.Id, Arg.Any<CancellationToken>()).Returns(othersNote);

            var result = await CreateHandler().Handle(new DeleteTextNoteCommand(othersNote.Id), CancellationToken.None);

            result.Error.ShouldBe(TextNoteErrors.Forbidden);
            await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_OwnedNote_SoftDeletesAndSavesOnce()
        {
            var note = TextNote.Create(_currentUser.Id, "T", "B");
            _notes.GetByIdAsync(note.Id, Arg.Any<CancellationToken>()).Returns(note);

            var result = await CreateHandler().Handle(new DeleteTextNoteCommand(note.Id), CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
            note.DeletedAtUtc.ShouldNotBeNull();
            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}
