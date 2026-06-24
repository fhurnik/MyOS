using MyOS.Core.Application.Abstractions;
using MyOS.Modules.Notes.Application.Notes.TextNotes;
using MyOS.Modules.Notes.Domain.Notes.TextNotes;
using MyOS.UnitTests.Common;

namespace MyOS.UnitTests.Application.Notes.TextNotes
{
    public class CreateTextNoteCommandHandlerTests
    {
        private readonly ITextNoteRepository _notes = Substitute.For<ITextNoteRepository>();
        private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
        private readonly FakeCurrentUser _currentUser = new();

        private CreateTextNoteCommandHandler CreateHandler() =>
            new(_notes, _currentUser, _unitOfWork);

        [Fact]
        public async Task Handle_ValidCommand_PersistsNoteOwnedByCurrentUserAndSavesOnce()
        {
            TextNote? added = null;
            await _notes.AddAsync(Arg.Do<TextNote>(n => added = n), Arg.Any<CancellationToken>());

            var result = await CreateHandler().Handle(new CreateTextNoteCommand("Title", "Body"), CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
            added.ShouldNotBeNull();
            added!.UserId.ShouldBe(_currentUser.Id);
            result.Value.ShouldBe(added.Id);
            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}
