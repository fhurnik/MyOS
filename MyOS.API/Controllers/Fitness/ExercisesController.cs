using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using MyOS.API.Controllers.Fitness.Requests;
using MyOS.Modules.Fitness.Application.Exercises;

namespace MyOS.API.Controllers.Fitness
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/fitness/exercises")]
    public sealed class ExercisesController(IMediator sender) : ApiControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] GetExercisesRequest request,
            CancellationToken cancellationToken)
        {
            var result = await sender.Send(
                new GetExercisesQuery(request, request.ActivityType, request.StrengthCategory),
                cancellationToken);
            return HandleResult(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var result = await sender.Send(new GetExerciseQuery(id), cancellationToken);
            return HandleResult(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateExerciseRequest request,
            CancellationToken cancellationToken)
        {
            var result = request switch
            {
                CreateCardioExerciseRequest c =>
                    await sender.Send(new CreateCardioExerciseCommand(c.Name, c.Distance), cancellationToken),
                CreateStrengthExerciseRequest s =>
                    await sender.Send(new CreateStrengthExerciseCommand(s.Name, s.Category), cancellationToken),
                _ => throw new InvalidOperationException("Unknown exercise type.")
            };
            return HandleResult(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateExerciseRequest request,
            CancellationToken cancellationToken)
        {
            var result = request switch
            {
                UpdateCardioExerciseRequest c =>
                    await sender.Send(new UpdateCardioExerciseCommand(id, c.Name, c.Distance), cancellationToken),
                UpdateStrengthExerciseRequest s =>
                    await sender.Send(new UpdateStrengthExerciseCommand(id, s.Name, s.Category), cancellationToken),
                _ => throw new InvalidOperationException("Unknown exercise type.")
            };
            return HandleResult(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            var result = await sender.Send(new DeleteExerciseCommand(id), cancellationToken);
            return HandleResult(result);
        }
    }
}
