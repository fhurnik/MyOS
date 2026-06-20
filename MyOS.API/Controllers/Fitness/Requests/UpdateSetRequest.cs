using System.Text.Json.Serialization;

namespace MyOS.API.Controllers.Fitness.Requests
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "category")]
    [JsonDerivedType(typeof(UpdateWeightedSetRequest), "weighted")]
    [JsonDerivedType(typeof(UpdateBodyweightSetRequest), "bodyweight")]
    public abstract record UpdateSetRequest
    {
        public int Reps { get; init; }
        public byte? Rir { get; init; }
    }

    public sealed record UpdateWeightedSetRequest : UpdateSetRequest
    {
        public decimal Weight { get; init; }
    }

    public sealed record UpdateBodyweightSetRequest : UpdateSetRequest
    {
        public decimal? AddedWeight { get; init; }
        public int? Negatives { get; init; }
    }
}
