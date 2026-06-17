using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using MyOS.API.Controllers.Storage.Requests;
using MyOS.Modules.Storage.Application.Files;

namespace MyOS.API.Controllers.Storage
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/storage/files")]
    public sealed class FilesController(IMediator sender) : ApiControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var result = await sender.Send(new GetFilesQuery(), cancellationToken);
            return HandleResult(result);
        }

        [HttpGet("trash")]
        public async Task<IActionResult> GetTrash(CancellationToken cancellationToken)
        {
            var result = await sender.Send(new GetDeletedFilesQuery(), cancellationToken);
            return HandleResult(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var result = await sender.Send(new GetFileQuery(id), cancellationToken);
            return HandleResult(result);
        }

        /// <summary>Serves the file inline (for in-browser playback/preview of audio, video, pdf, …).</summary>
        [HttpGet("{id}/content")]
        public Task<IActionResult> GetContent(Guid id, CancellationToken cancellationToken) =>
            ServeFileAsync(id, inline: true, cancellationToken);

        /// <summary>Serves the file as an attachment (forces a download with the original file name).</summary>
        [HttpGet("{id}/download")]
        public Task<IActionResult> Download(Guid id, CancellationToken cancellationToken) =>
            ServeFileAsync(id, inline: false, cancellationToken);

        private async Task<IActionResult> ServeFileAsync(Guid id, bool inline, CancellationToken cancellationToken)
        {
            var result = await sender.Send(new GetFileForDownloadQuery(id, inline), cancellationToken);

            if (result.IsFailure)
                return HandleResult(result);

            var file = result.Value;

            // Range processing enabled for both → seeking/progressive playback and resumable downloads.
            return inline
                ? PhysicalFile(file.AbsolutePath, file.ContentType, enableRangeProcessing: true)
                : PhysicalFile(file.AbsolutePath, file.ContentType, file.OriginalName, enableRangeProcessing: true);
        }

        [HttpPost]
        public async Task<IActionResult> Upload(
            IFormFile file,
            [FromQuery] Guid? folderId,
            CancellationToken cancellationToken)
        {
            await using var content = file.OpenReadStream();

            var result = await sender.Send(
                new UploadFileCommand(content, file.FileName, file.ContentType, file.Length, folderId),
                cancellationToken);

            return HandleResult(result);
        }

        [HttpPut("{id}/move")]
        public async Task<IActionResult> Move(
            Guid id,
            [FromBody] MoveFileRequest request,
            CancellationToken cancellationToken)
        {
            var result = await sender.Send(new MoveFileCommand(id, request.FolderId), cancellationToken);
            return HandleResult(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            var result = await sender.Send(new DeleteFileCommand(id), cancellationToken);
            return HandleResult(result);
        }

        [HttpPost("{id}/restore")]
        public async Task<IActionResult> Restore(Guid id, CancellationToken cancellationToken)
        {
            var result = await sender.Send(new RestoreFileCommand(id), cancellationToken);
            return HandleResult(result);
        }
    }
}
