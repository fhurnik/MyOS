using MyOS.Core.Domain.Enums;

namespace MyOS.API.Controllers.Identity.Requests
{
    public sealed record ChangeLanguageRequest(Language Language, string RefreshToken);
}
