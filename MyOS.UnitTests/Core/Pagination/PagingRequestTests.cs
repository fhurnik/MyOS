using MyOS.Core.Application.Abstractions.Pagination;

namespace MyOS.UnitTests.Core.Pagination
{
    // Skip/Take are the paging math the SQLKata layer depends on: Take clamps PageSize into
    // [1, 100] and Skip is derived from the *clamped* Take and never goes negative.
    public class PagingRequestTests
    {
        [Theory]
        [InlineData(10, 10)]   // within range, untouched
        [InlineData(0, 1)]     // floored to 1
        [InlineData(-5, 1)]    // negative floored to 1
        [InlineData(100, 100)] // upper bound untouched
        [InlineData(500, 100)] // capped to MaxPageSize
        public void Take_ClampsPageSizeIntoAllowedRange(int pageSize, int expectedTake)
        {
            new PagingRequest { PageSize = pageSize }.Take.ShouldBe(expectedTake);
        }

        [Theory]
        [InlineData(1, 10, 0)]    // first page → no offset
        [InlineData(3, 10, 20)]   // (3-1) * 10
        [InlineData(0, 10, 0)]    // page 0 guarded to non-negative offset
        [InlineData(-2, 10, 0)]   // negative page guarded
        [InlineData(2, 500, 100)] // offset uses the clamped Take (100), not raw PageSize
        public void Skip_UsesClampedTakeAndNeverGoesNegative(int page, int pageSize, int expectedSkip)
        {
            new PagingRequest { Page = page, PageSize = pageSize }.Skip.ShouldBe(expectedSkip);
        }
    }
}
