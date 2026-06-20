using MyOS.Core.Application.Abstractions;
using MyOS.Core.Application.Abstractions.Messaging;
using MyOS.Core.Application.Abstractions.Results;
using MyOS.Modules.Fitness.Application.Stats.Shared;
using SqlKata.Execution;

namespace MyOS.Modules.Fitness.Application.Stats
{
    public sealed record GetUserDashboardQuery : IQuery<UserDashboardDto>;

    internal sealed class GetUserDashboardQueryHandler(
        QueryFactory db,
        ICurrentUser currentUser) : IQueryHandler<GetUserDashboardQuery, UserDashboardDto>
    {
        public async Task<Result<UserDashboardDto>> Handle(GetUserDashboardQuery query, CancellationToken cancellationToken)
        {
            var dashboard = await db.Query("fitness.v_user_fitness_dashboard")
                .Where("user_id", currentUser.Id)
                .FirstOrDefaultAsync<UserDashboardDto>(cancellationToken: cancellationToken);

            // No workouts yet → no row in the view; return an empty dashboard.
            return Result<UserDashboardDto>.Success(dashboard ?? new UserDashboardDto());
        }
    }
}
