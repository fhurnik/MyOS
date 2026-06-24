using MediatR;
using MyOS.Core.Application.Abstractions.BusinessRules;
using MyOS.Core.Application.Abstractions.Results;

namespace MyOS.UnitTests.Core.BusinessRules
{
    public class BusinessRuleCheckerTests
    {
        // Records whether it was evaluated so we can prove short-circuiting.
        private sealed class FakeRule(bool passes, Error error) : IBusinessRule
        {
            public int Evaluations { get; private set; }
            public Error Error { get; } = error;

            public Task<bool> CheckAsync(CancellationToken cancellationToken)
            {
                Evaluations++;
                return Task.FromResult(passes);
            }
        }

        private static readonly Error FirstError = Error.Failure("Test.First");
        private static readonly Error SecondError = Error.Failure("Test.Second");

        [Fact]
        public async Task CheckAsync_AllRulesPass_ReturnsSuccess()
        {
            var result = await BusinessRuleChecker.CheckAsync(CancellationToken.None,
                new FakeRule(passes: true, FirstError),
                new FakeRule(passes: true, SecondError));

            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task CheckAsync_NoRules_ReturnsSuccess()
        {
            var result = await BusinessRuleChecker.CheckAsync(CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task CheckAsync_FirstRuleFails_ReturnsThatErrorAndDoesNotEvaluateLaterRules()
        {
            var failing = new FakeRule(passes: false, FirstError);
            var later = new FakeRule(passes: true, SecondError);

            var result = await BusinessRuleChecker.CheckAsync(CancellationToken.None, failing, later);

            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldBe(FirstError);
            later.Evaluations.ShouldBe(0); // short-circuited on the first failure
        }

        [Fact]
        public async Task CheckAsync_OnlyLaterRuleFails_ReturnsTheLaterRulesError()
        {
            var result = await BusinessRuleChecker.CheckAsync(CancellationToken.None,
                new FakeRule(passes: true, FirstError),
                new FakeRule(passes: false, SecondError));

            result.Error.ShouldBe(SecondError);
        }
    }
}
