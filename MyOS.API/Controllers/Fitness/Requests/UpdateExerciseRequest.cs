using System.Text.Json.Serialization;
using MyOS.Modules.Fitness.Domain.Exercises;

namespace MyOS.API.Controllers.Fitness.Requests
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "activityType")]
    [JsonDerivedType(typeof(UpdateCardioExerciseRequest), "cardio")]
    [JsonDerivedType(typeof(UpdateStrengthExerciseRequest), "strength")]
    public abstract record UpdateExerciseRequest
    {
        public string Name { get; init; } = string.Empty;
    }

    public sealed record UpdateCardioExerciseRequest : UpdateExerciseRequest
    {
        public int Distance { get; init; }
    }

    public sealed record UpdateStrengthExerciseRequest : UpdateExerciseRequest
    {
        public StrengthCategory Category { get; init; }
    }
}
