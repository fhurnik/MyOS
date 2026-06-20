namespace MyOS.Modules.Fitness.Application.Stats.Shared
{
    public sealed record WeeklySetsDto
    {
        public Guid ExerciseId { get; init; }
        public string ExerciseName { get; init; } = string.Empty;
        public int IsoYear { get; init; }
        public int IsoWeek { get; init; }
        public int SetCount { get; init; }
    }
}
