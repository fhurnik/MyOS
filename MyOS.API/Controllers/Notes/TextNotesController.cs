using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using MyOS.API.Controllers.Notes.Requests;
using MyOS.Modules.Notes.Application.Notes.TextNotes;

namespace MyOS.API.Controllers.Notes
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/notes/text")]
    public sealed class TextNotesController(IMediator sender) : ApiControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] GetTextNotesRequest request,
            CancellationToken cancellationToken)
        {
            var result = await sender.Send(new GetTextNotesQuery(request), cancellationToken);
            return HandleResult(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(
            Guid id,
            CancellationToken cancellationToken)
        {
            var result = await sender.Send(new GetTextNoteQuery(id), cancellationToken);
            return HandleResult(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateTextNoteRequest request,
            CancellationToken cancellationToken)
        {
            var result = await sender.Send(
                new CreateTextNoteCommand(request.Title, request.Text),
                cancellationToken);
            return HandleResult(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateTextNoteRequest request,
            CancellationToken cancellationToken)
        {
            var result = await sender.Send(
                new UpdateTextNoteCommand(id, request.Title, request.Text),
                cancellationToken);
            return HandleResult(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(
            Guid id,
            CancellationToken cancellationToken)
        {
            var result = await sender.Send(new DeleteTextNoteCommand(id), cancellationToken);
            return HandleResult(result);
        }
    }
}
