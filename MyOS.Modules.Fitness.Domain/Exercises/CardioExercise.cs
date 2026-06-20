namespace MyOS.Modules.Fitness.Domain.Exercises
{
    public sealed class CardioExercise : Exercise
    {
        public int Distance { get; private set; }

        public static CardioExercise Create(Guid userId, string name, int distance) =>
            new(userId, name, distance);

        internal CardioExercise(Guid userId, string name, int distance)
            : base(userId, name, ActivityType.Cardio)
        {
            Distance = distance;
        }

        internal void ChangeDistance(int distance)
        {
            Distance = distance;
            Touch();
        }

        private CardioExercise()
        {
            // for EF Core
        }
    }
}
