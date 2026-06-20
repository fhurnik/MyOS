using System.Text.Json.Serialization;
using MyOS.Modules.Fitness.Domain.Exercises;

namespace MyOS.API.Controllers.Fitness.Requests
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "activityType")]
    [JsonDerivedType(typeof(CreateCardioExerciseRequest), "cardio")]
    [JsonDerivedType(typeof(CreateStrengthExerciseRequest), "strength")]
    public abstract record CreateExerciseRequest
    {
        public string Name { get; init; } = string.Empty;
    }

    public sealed record CreateCardioExerciseRequest : CreateExerciseRequest
    {
        public int Distance { get; init; }
    }

    public sealed record CreateStrengthExerciseRequest : CreateExerciseRequest
    {
        public StrengthCategory Category { get; init; }
    }
}
