using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using MyOS.API.Controllers.Storage.Requests;
using MyOS.Modules.Storage.Application.Folders;

namespace MyOS.API.Controllers.Storage
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/storage/folders")]
    public sealed class FoldersController(IMediator sender) : ApiControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var result = await sender.Send(new GetFoldersQuery(), cancellationToken);
            return HandleResult(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateFolderRequest request,
            CancellationToken cancellationToken)
        {
            var result = await sender.Send(new CreateFolderCommand(request.Name, request.ParentId), cancellationToken);
            return HandleResult(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Rename(
            Guid id,
            [FromBody] RenameFolderRequest request,
            CancellationToken cancellationToken)
        {
            var result = await sender.Send(new RenameFolderCommand(id, request.Name), cancellationToken);
            return HandleResult(result);
        }

        [HttpPut("{id}/move")]
        public async Task<IActionResult> Move(
            Guid id,
            [FromBody] MoveFolderRequest request,
            CancellationToken cancellationToken)
        {
            var result = await sender.Send(new MoveFolderCommand(id, request.ParentId), cancellationToken);
            return HandleResult(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            var result = await sender.Send(new DeleteFolderCommand(id), cancellationToken);
            return HandleResult(result);
        }
    }
}
