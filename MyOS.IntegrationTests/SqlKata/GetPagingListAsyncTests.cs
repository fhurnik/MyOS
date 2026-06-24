using MyOS.Core.Application.Abstractions.Pagination;
using MyOS.Core.Application.Errors;
using MyOS.Core.Application.SqlKata;
using MyOS.IntegrationTests.Infrastructure;
using SqlKata.Execution;

namespace MyOS.IntegrationTests.SqlKata
{
    // Exercises the one piece of read-side C# we own — GetPagingListAsync — against a real SQL
    // Server + real view. Covers what unit tests structurally cannot: that snake_case OrderBy maps
    // onto actual columns, that paging/clamp execute, and that COUNT + paged SELECT agree.
    [Trait("Category", "Integration")]
    [Collection(DatabaseCollection.Name)]
    public class GetPagingListAsyncTests(SqlServerFixture fixture)
    {
        // Property-based record (parameterless ctor) so Dapper can materialize rows from the view.
        private sealed record TextNoteRow
        {
            public Guid Id { get; init; }
            public Guid UserId { get; init; }
            public string Title { get; init; } = string.Empty;
            public string Text { get; init; } = string.Empty;
            public DateTime CreatedAtUtc { get; init; }
            public DateTime? UpdatedAtUtc { get; init; }
        }

        private static async Task SeedNoteAsync(QueryFactory db, Guid userId, string title, DateTime createdAtUtc) =>
            await db.Query("notes.text_notes").InsertAsync(new
            {
                id = Guid.NewGuid(),
                user_id = userId,
                title,
                text = "body",
                created_at_utc = createdAtUtc
            });

        [Fact]
        public async Task GetPagingListAsync_ReturnsRequestedPageSliceAndTotalCount()
        {
            var db = fixture.CreateQueryFactory();
            var userId = Guid.NewGuid();
            var baseTime = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            for (var i = 0; i < 25; i++)
                await SeedNoteAsync(db, userId, $"Note {i:D2}", baseTime.AddMinutes(i));

            var request = new PagingRequest { Page = 2, PageSize = 10, OrderBy = "title" };
            var result = await db.Query("notes.v_text_notes").Where("user_id", userId)
                .GetPagingListAsync<TextNoteRow>(request, CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
            result.Value.TotalCount.ShouldBe(25);   // COUNT ignores paging
            result.Value.Page.ShouldBe(2);
            result.Value.Items.Count.ShouldBe(10);
            // title asc, zero-padded so lexical == numeric → page 2 is items 10..19
            result.Value.Items.First().Title.ShouldBe("Note 10");
            result.Value.Items.Last().Title.ShouldBe("Note 19");
        }

        [Fact]
        public async Task GetPagingListAsync_PageSizeAboveMax_IsClampedTo100()
        {
            var db = fixture.CreateQueryFactory();
            var userId = Guid.NewGuid();
            for (var i = 0; i < 3; i++)
                await SeedNoteAsync(db, userId, $"N{i}", DateTime.UtcNow);

            var request = new PagingRequest { Page = 1, PageSize = 500, OrderBy = "title" };
            var result = await db.Query("notes.v_text_notes").Where("user_id", userId)
                .GetPagingListAsync<TextNoteRow>(request, CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
            result.Value.PageSize.ShouldBe(100);    // Take clamped to MaxPageSize
            result.Value.Items.Count.ShouldBe(3);
        }

        [Fact]
        public async Task GetPagingListAsync_OrderByCamelCaseProperty_MapsToSnakeCaseColumnAndSorts()
        {
            var db = fixture.CreateQueryFactory();
            var userId = Guid.NewGuid();
            var t = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc);
            await SeedNoteAsync(db, userId, "middle", t.AddDays(1));
            await SeedNoteAsync(db, userId, "newest", t.AddDays(2));
            await SeedNoteAsync(db, userId, "oldest", t.AddDays(0));

            // "createdAtUtc" must lower to the real column "created_at_utc" and actually order rows.
            var request = new PagingRequest { Page = 1, PageSize = 10, OrderBy = "createdAtUtc", OrderByDesc = true };
            var result = await db.Query("notes.v_text_notes").Where("user_id", userId)
                .GetPagingListAsync<TextNoteRow>(request, CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
            result.Value.Items.Select(n => n.Title).ShouldBe(["newest", "middle", "oldest"]);
        }

        [Fact]
        public async Task GetPagingListAsync_OrderByUnknownColumn_FailsWithInvalidOrderBy()
        {
            var db = fixture.CreateQueryFactory();

            var request = new PagingRequest { OrderBy = "definitelyNotAColumn" };
            var result = await db.Query("notes.v_text_notes")
                .GetPagingListAsync<TextNoteRow>(request, CancellationToken.None);

            result.IsFailure.ShouldBeTrue();
            result.Error.Code.ShouldBe(PagingErrors.InvalidOrderBy.Code);
        }
    }
}
