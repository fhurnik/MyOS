using MediatR;
using MyOS.Core.Application.Abstractions;
using MyOS.Core.Application.DomainEvents;
using MyOS.Core.Application.Exceptions;
using MyOS.Core.Domain.Enums;
using MyOS.Identity.Application.Abstractions;
using MyOS.Identity.Application.Commands.Register;
using MyOS.Identity.Application.Errors;
using MyOS.Identity.Domain.Users;
using NSubstitute.ExceptionExtensions;

namespace MyOS.UnitTests.Application.Identity.Register
{
    public class RegisterCommandHandlerTests
    {
        private readonly IUserRepository _users = Substitute.For<IUserRepository>();
        private readonly IPasswordHasher _hasher = Substitute.For<IPasswordHasher>();
        private readonly IPublisher _publisher = Substitute.For<IPublisher>();
        private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

        private RegisterCommandHandler CreateHandler() =>
            new(_users, _hasher, _publisher, _unitOfWork);

        private static RegisterCommand Command(string email = "new@myos.local") =>
            new("Jan", "Kowalski", email, "Password1", Language.English);

        public RegisterCommandHandlerTests()
        {
            _hasher.Hash(Arg.Any<string>()).Returns("hashed");
        }

        [Fact]
        public async Task Handle_EmailAlreadyTaken_ReturnsConflictAndDoesNotSaveOrPublish()
        {
            var existing = User.Create("A", "B", "taken@myos.local", "h");
            _users.GetByEmailAsync("taken@myos.local", Arg.Any<CancellationToken>()).Returns(existing);

            var result = await CreateHandler().Handle(Command("taken@myos.local"), CancellationToken.None);

            result.Error.Code.ShouldBe(UserErrors.EmailAlreadyInUse.Code);
            await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
            await _publisher.DidNotReceive().Publish(Arg.Any<UserRegisteredEvent>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_UniqueConstraintRaceOnSave_ReturnsConflictAndDoesNotPublish()
        {
            _users.GetByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((User?)null);
            _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
                .ThrowsAsync(new UniqueConstraintViolationException(new Exception()));

            var result = await CreateHandler().Handle(Command(), CancellationToken.None);

            result.Error.Code.ShouldBe(UserErrors.EmailAlreadyInUse.Code);
            await _publisher.DidNotReceive().Publish(Arg.Any<UserRegisteredEvent>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_NewEmail_PersistsUserSavesOnceAndPublishesAfterCommit()
        {
            _users.GetByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((User?)null);

            var result = await CreateHandler().Handle(Command(), CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
            await _users.Received(1).AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
            await _publisher.Received(1).Publish(
                Arg.Is<UserRegisteredEvent>(e => e.UserId == result.Value), Arg.Any<CancellationToken>());
        }
    }
}
