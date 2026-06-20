namespace MyOS.Modules.Fitness.Application.Stats.Shared
{
    public sealed record ProgressionPointDto
    {
        public DateTime Date { get; init; }
        public decimal? Value { get; init; }
    }
}
