namespace MyOS.Modules.Fitness.Application.Workouts.Shared
{
    // Flat set payload used for inline bulk sets when adding a strength exercise. The category is
    // fixed by the referenced exercise, so the handler picks the weighted/bodyweight factory.
    public sealed record SetInput(int Reps, decimal? Weight, decimal? AddedWeight, int? Negatives, byte? Rir);
}
