using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using MyOS.API.Controllers.Identity.Requests;
using MyOS.Identity.Application.Commands.ChangeLanguage;

namespace MyOS.API.Controllers.Identity
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/users")]
    public sealed class UsersController(IMediator sender) : ApiControllerBase
    {
        [HttpPatch("me/language")]
        public async Task<IActionResult> ChangeLanguage(
            [FromBody] ChangeLanguageRequest request,
            CancellationToken cancellationToken)
        {
            var result = await sender.Send(
                new ChangeLanguageCommand(request.Language, request.RefreshToken),
                cancellationToken);
            return HandleResult(result);
        }
    }
}
