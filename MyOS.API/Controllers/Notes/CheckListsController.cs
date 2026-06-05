using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using MyOS.API.Controllers.Notes.Requests;
using MyOS.Modules.Notes.Application.Notes.CheckList;

namespace MyOS.API.Controllers.Notes
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/notes/checklists")]
    public sealed class CheckListsController(IMediator sender) : ApiControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] GetCheckListsRequest request,
            CancellationToken cancellationToken)
        {
            var result = await sender.Send(new GetCheckListsQuery(request), cancellationToken);
            return HandleResult(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(
            Guid id,
            CancellationToken cancellationToken)
        {
            var result = await sender.Send(new GetCheckListQuery(id), cancellationToken);
            return HandleResult(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateCheckListRequest request,
            CancellationToken cancellationToken)
        {
            var result = await sender.Send(
                new CreateCheckListCommand(request.Title),
                cancellationToken);
            return HandleResult(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTitle(
            Guid id,
            [FromBody] UpdateCheckListTitleRequest request,
            CancellationToken cancellationToken)
        {
            var result = await sender.Send(
                new UpdateCheckListTitleCommand(id, request.Title),
                cancellationToken);
            return HandleResult(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(
            Guid id,
            CancellationToken cancellationToken)
        {
            var result = await sender.Send(new DeleteCheckListCommand(id), cancellationToken);
            return HandleResult(result);
        }

        [HttpPost("{id}/items")]
        public async Task<IActionResult> AddItem(
            Guid id,
            [FromBody] AddCheckListItemRequest request,
            CancellationToken cancellationToken)
        {
            var result = await sender.Send(
                new AddCheckListItemCommand(id, request.Text),
                cancellationToken);
            return HandleResult(result);
        }

        [HttpPut("{id}/items/{itemId}")]
        public async Task<IActionResult> UpdateItem(
            Guid id,
            Guid itemId,
            [FromBody] UpdateCheckListItemRequest request,
            CancellationToken cancellationToken)
        {
            var result = await sender.Send(
                new UpdateCheckListItemCommand(id, itemId, request.Text),
                cancellationToken);
            return HandleResult(result);
        }

        [HttpDelete("{id}/items/{itemId}")]
        public async Task<IActionResult> RemoveItem(
            Guid id,
            Guid itemId,
            CancellationToken cancellationToken)
        {
            var result = await sender.Send(new RemoveCheckListItemCommand(id, itemId), cancellationToken);
            return HandleResult(result);
        }

        [HttpPatch("{id}/items/{itemId}/toggle")]
        public async Task<IActionResult> ToggleItem(
            Guid id,
            Guid itemId,
            CancellationToken cancellationToken)
        {
            var result = await sender.Send(new ToggleCheckListItemCommand(id, itemId), cancellationToken);
            return HandleResult(result);
        }

        [HttpPatch("{id}/items/{itemId}/reorder")]
        public async Task<IActionResult> ReorderItem(
            Guid id,
            Guid itemId,
            [FromBody] ReorderCheckListItemRequest request,
            CancellationToken cancellationToken)
        {
            var result = await sender.Send(
                new ReorderCheckListItemCommand(id, itemId, request.NewOrder),
                cancellationToken);
            return HandleResult(result);
        }
    }
}
