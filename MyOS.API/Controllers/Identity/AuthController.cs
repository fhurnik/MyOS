using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyOS.API.Controllers.Identity.Requests;
using MyOS.Identity.Application.Commands.Login;
using MyOS.Identity.Application.Commands.RefreshTokens;
using MyOS.Identity.Application.Commands.Register;

namespace MyOS.API.Controllers.Identity
{
    [AllowAnonymous]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/auth")]
    public sealed class AuthController(IMediator sender) : ApiControllerBase
    {
        [HttpPost("register")]
        public async Task<IActionResult> Register(
            [FromBody] RegisterRequest request,
            CancellationToken cancellationToken)
        {
            var result = await sender.Send(
                new RegisterCommand(request.FirstName, request.LastName, request.Email, request.Password, CurrentUser.Language),
                cancellationToken);
            return HandleResult(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(
            [FromBody] LoginRequest request,
            CancellationToken cancellationToken)
        {
            var result = await sender.Send(
                new LoginCommand(request.Email, request.Password),
                cancellationToken);
            return HandleResult(result);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(
            [FromBody] RefreshTokenRequest request,
            CancellationToken cancellationToken)
        {
            var result = await sender.Send(
                new RefreshTokenCommand(request.Token),
                cancellationToken);
            return HandleResult(result);
        }
    }
}
