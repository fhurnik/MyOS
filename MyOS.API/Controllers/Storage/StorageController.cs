using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using MyOS.Modules.Storage.Application.Quotas;

namespace MyOS.API.Controllers.Storage
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/storage")]
    public sealed class StorageController(IMediator sender) : ApiControllerBase
    {
        [HttpGet("quota")]
        public async Task<IActionResult> GetQuota(CancellationToken cancellationToken)
        {
            var result = await sender.Send(new GetQuotaQuery(), cancellationToken);
            return HandleResult(result);
        }
    }
}
