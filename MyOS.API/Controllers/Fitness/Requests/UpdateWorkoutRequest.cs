namespace MyOS.API.Controllers.Fitness.Requests
{
    public sealed record UpdateWorkoutRequest(DateOnly Date, string? Notes);
}
