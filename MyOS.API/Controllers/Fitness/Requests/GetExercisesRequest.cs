using MyOS.Core.Application.Abstractions.Pagination;
using MyOS.Modules.Fitness.Domain.Exercises;

namespace MyOS.API.Controllers.Fitness.Requests
{
    public sealed record GetExercisesRequest : PagingRequest
    {
        public ActivityType? ActivityType { get; init; }
        public StrengthCategory? StrengthCategory { get; init; }
    }
}
