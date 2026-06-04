using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyOS.API.Extensions;
using MyOS.Core.Application.Abstractions;
using MyOS.Core.Application.Abstractions.Results;

namespace MyOS.API.Controllers
{
    [ApiController]
    [Authorize]
    public abstract class ApiControllerBase : ControllerBase
    {
        private IErrorTranslator? _errorTranslator;
        private ICurrentUser? _currentUser;

        protected IErrorTranslator ErrorTranslator =>
            _errorTranslator ??= HttpContext.RequestServices.GetRequiredService<IErrorTranslator>();

        protected ICurrentUser CurrentUser =>
            _currentUser ??= HttpContext.RequestServices.GetRequiredService<ICurrentUser>();

        protected IActionResult HandleResult<T>(Result<T> result)
        {
            return result.ToActionResult(HttpContext, ErrorTranslator, CurrentUser);
        }
    }
}
