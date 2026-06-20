namespace MyOS.API.Controllers.Fitness.Requests
{
    public sealed record CreateWorkoutRequest(DateOnly Date, string? Notes);
}
