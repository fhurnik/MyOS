namespace MyOS.Modules.Fitness.Domain.Exercises
{
    public sealed class StrengthExercise : Exercise
    {
        public StrengthCategory StrengthCategory { get; private set; }

        public static StrengthExercise Create(Guid userId, string name, StrengthCategory category) =>
            new(userId, name, category);

        internal StrengthExercise(Guid userId, string name, StrengthCategory category)
            : base(userId, name, ActivityType.Strength)
        {
            StrengthCategory = category;
        }

        internal void ChangeCategory(StrengthCategory category)
        {
            StrengthCategory = category;
            Touch();
        }

        private StrengthExercise()
        {
            // for EF Core
        }
    }
}
