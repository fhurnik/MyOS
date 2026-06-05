namespace MyOS.API.Controllers.Identity.Requests
{
    public sealed record RegisterRequest(string FirstName, string LastName, string Email, string Password);
}
