namespace MyOS.Modules.Fitness.Application.Stats.Shared
{
    public sealed record ProgressionPointDto
    {
        public DateOnly Date { get; init; }
        public decimal? Value { get; init; }
    }
}
