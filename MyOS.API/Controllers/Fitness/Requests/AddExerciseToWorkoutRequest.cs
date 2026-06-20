using System.Text.Json.Serialization;

namespace MyOS.API.Controllers.Fitness.Requests
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "activityType")]
    [JsonDerivedType(typeof(AddCardioExerciseRequest), "cardio")]
    [JsonDerivedType(typeof(AddStrengthExerciseRequest), "strength")]
    public abstract record AddExerciseToWorkoutRequest
    {
        public Guid ExerciseId { get; init; }
    }

    public sealed record AddCardioExerciseRequest : AddExerciseToWorkoutRequest
    {
        public int Duration { get; init; }
    }

    public sealed record AddStrengthExerciseRequest : AddExerciseToWorkoutRequest
    {
        // Optional inline bulk sets; category is fixed by the referenced exercise.
        public IReadOnlyList<SetInputRequest> Sets { get; init; } = [];
    }

    public sealed record SetInputRequest(int Reps, decimal? Weight, decimal? AddedWeight, int? Negatives, byte? Rir);
}
