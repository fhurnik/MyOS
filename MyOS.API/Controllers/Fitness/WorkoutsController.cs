using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using MyOS.API.Controllers.Fitness.Requests;
using MyOS.Modules.Fitness.Application.Workouts;
using MyOS.Modules.Fitness.Application.Workouts.Shared;

namespace MyOS.API.Controllers.Fitness
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/fitness/workouts")]
    public sealed class WorkoutsController(IMediator sender) : ApiControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] GetWorkoutsRequest request,
            CancellationToken cancellationToken)
        {
            var result = await sender.Send(new GetWorkoutsQuery(request), cancellationToken);
            return HandleResult(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var result = await sender.Send(new GetWorkoutQuery(id), cancellationToken);
            return HandleResult(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateWorkoutRequest request,
            CancellationToken cancellationToken)
        {
            var result = await sender.Send(new CreateWorkoutCommand(request.Date, request.Notes), cancellationToken);
            return HandleResult(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateWorkoutRequest request,
            CancellationToken cancellationToken)
        {
            var result = await sender.Send(new UpdateWorkoutCommand(id, request.Date, request.Notes), cancellationToken);
            return HandleResult(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            var result = await sender.Send(new DeleteWorkoutCommand(id), cancellationToken);
            return HandleResult(result);
        }

        [HttpPost("{id}/exercises")]
        public async Task<IActionResult> AddExercise(
            Guid id,
            [FromBody] AddExerciseToWorkoutRequest request,
            CancellationToken cancellationToken)
        {
            var result = request switch
            {
                AddCardioExerciseRequest c =>
                    await sender.Send(new AddCardioExerciseToWorkoutCommand(id, c.ExerciseId, c.Duration), cancellationToken),
                AddStrengthExerciseRequest s =>
                    await sender.Send(new AddStrengthExerciseToWorkoutCommand(
                        id,
                        s.ExerciseId,
                        s.Sets.Select(x => new SetInput(x.Reps, x.Weight, x.AddedWeight, x.Negatives, x.Rir)).ToList()),
                        cancellationToken),
                _ => throw new InvalidOperationException("Unknown exercise type.")
            };
            return HandleResult(result);
        }

        [HttpDelete("{id}/exercises/{workoutExerciseId}")]
        public async Task<IActionResult> RemoveExercise(
            Guid id,
            Guid workoutExerciseId,
            CancellationToken cancellationToken)
        {
            var result = await sender.Send(new RemoveWorkoutExerciseCommand(id, workoutExerciseId), cancellationToken);
            return HandleResult(result);
        }

        [HttpPatch("{id}/exercises/{workoutExerciseId}")]
        public async Task<IActionResult> UpdateDuration(
            Guid id,
            Guid workoutExerciseId,
            [FromBody] UpdateCardioDurationRequest request,
            CancellationToken cancellationToken)
        {
            var result = await sender.Send(
                new UpdateCardioDurationCommand(id, workoutExerciseId, request.Duration),
                cancellationToken);
            return HandleResult(result);
        }

        [HttpPost("{id}/exercises/{workoutExerciseId}/sets")]
        public async Task<IActionResult> AddSet(
            Guid id,
            Guid workoutExerciseId,
            [FromBody] AddSetRequest request,
            CancellationToken cancellationToken)
        {
            var result = request switch
            {
                AddWeightedSetRequest w =>
                    await sender.Send(new AddWeightedSetCommand(id, workoutExerciseId, w.Reps, w.Weight, w.Rir), cancellationToken),
                AddBodyweightSetRequest b =>
                    await sender.Send(new AddBodyweightSetCommand(id, workoutExerciseId, b.Reps, b.AddedWeight, b.Negatives, b.Rir), cancellationToken),
                _ => throw new InvalidOperationException("Unknown set category.")
            };
            return HandleResult(result);
        }

        [HttpPatch("{id}/exercises/{workoutExerciseId}/sets/{setId}")]
        public async Task<IActionResult> UpdateSet(
            Guid id,
            Guid workoutExerciseId,
            Guid setId,
            [FromBody] UpdateSetRequest request,
            CancellationToken cancellationToken)
        {
            var result = request switch
            {
                UpdateWeightedSetRequest w =>
                    await sender.Send(new UpdateWeightedSetCommand(id, workoutExerciseId, setId, w.Reps, w.Weight, w.Rir), cancellationToken),
                UpdateBodyweightSetRequest b =>
                    await sender.Send(new UpdateBodyweightSetCommand(id, workoutExerciseId, setId, b.Reps, b.AddedWeight, b.Negatives, b.Rir), cancellationToken),
                _ => throw new InvalidOperationException("Unknown set category.")
            };
            return HandleResult(result);
        }

        [HttpDelete("{id}/exercises/{workoutExerciseId}/sets/{setId}")]
        public async Task<IActionResult> RemoveSet(
            Guid id,
            Guid workoutExerciseId,
            Guid setId,
            CancellationToken cancellationToken)
        {
            var result = await sender.Send(new RemoveSetCommand(id, workoutExerciseId, setId), cancellationToken);
            return HandleResult(result);
        }
    }
}
