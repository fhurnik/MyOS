namespace MyOS.API.Controllers.Identity.Requests
{
    public sealed record LoginRequest(string Email, string Password);
}
