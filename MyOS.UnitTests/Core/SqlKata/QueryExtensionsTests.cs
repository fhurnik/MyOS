using MyOS.Core.Application.Abstractions.Pagination;
using MyOS.Core.Application.Errors;
using MyOS.Core.Application.SqlKata;
using SqlKata;

namespace MyOS.UnitTests.Core.SqlKata
{
    // GetPagingListAsync validates OrderBy against T's properties BEFORE touching the database,
    // so the rejection branch is unit-testable without a connection. The accepting branch runs
    // COUNT + SELECT against a real view and is deferred to integration tests.
    public class QueryExtensionsTests
    {
        private sealed record SampleDto
        {
            public Guid Id { get; init; }
            public DateTime CreatedAtUtc { get; init; }
        }

        [Fact]
        public async Task GetPagingListAsync_OrderByUnknownColumn_FailsWithoutQueryingDatabase()
        {
            var request = new PagingRequest { OrderBy = "doesNotExist" };

            var result = await new Query("t").GetPagingListAsync<SampleDto>(request, CancellationToken.None);

            result.IsFailure.ShouldBeTrue();
            result.Error.Code.ShouldBe(PagingErrors.InvalidOrderBy.Code);
        }

        [Fact]
        public async Task GetPagingListAsync_OrderByDiffersBeyondFirstCharacter_IsRejected()
        {
            // Matching is case-insensitive on the FIRST character only: "createdAtUtc" would match
            // "CreatedAtUtc", but the fully lower-cased "createdatutc" must not.
            var request = new PagingRequest { OrderBy = "createdatutc" };

            var result = await new Query("t").GetPagingListAsync<SampleDto>(request, CancellationToken.None);

            result.IsFailure.ShouldBeTrue();
            result.Error.Code.ShouldBe(PagingErrors.InvalidOrderBy.Code);
        }
    }
}
