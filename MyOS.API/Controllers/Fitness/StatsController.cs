using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using MyOS.Modules.Fitness.Application.Stats;

namespace MyOS.API.Controllers.Fitness
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/fitness")]
    public sealed class StatsController(IMediator sender) : ApiControllerBase
    {
        [HttpGet("stats/weekly-sets")]
        public async Task<IActionResult> GetWeeklySets(
            [FromQuery] Guid? exerciseId,
            CancellationToken cancellationToken)
        {
            var result = await sender.Send(new GetWeeklySetsQuery(exerciseId), cancellationToken);
            return HandleResult(result);
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard(CancellationToken cancellationToken)
        {
            var result = await sender.Send(new GetUserDashboardQuery(), cancellationToken);
            return HandleResult(result);
        }
    }
}
