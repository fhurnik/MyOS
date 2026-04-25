using Microsoft.AspNetCore.Mvc;
using MyOS.API.Extensions;
using MyOS.Core.Application.Abstractions.Results;

namespace MyOS.API.Controllers
{
    [ApiController]
    public abstract class ApiControllerBase : ControllerBase
    {
        protected IActionResult HandleResult<T>(Result<T> result)
        {
            return result.ToActionResult(HttpContext);
        }
    }
}
