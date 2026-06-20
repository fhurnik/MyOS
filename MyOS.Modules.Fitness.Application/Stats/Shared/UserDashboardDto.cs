namespace MyOS.Modules.Fitness.Application.Stats.Shared
{
    public sealed record UserDashboardDto
    {
        public DateOnly? LastWorkoutDate { get; init; }
        public int? DaysSinceLastWorkout { get; init; }
        public int WorkoutsThisWeek { get; init; }
        public int SetsThisWeek { get; init; }
    }
}
