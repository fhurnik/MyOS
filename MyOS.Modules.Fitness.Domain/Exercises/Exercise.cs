using MyOS.Core.Domain.Entities;

namespace MyOS.Modules.Fitness.Domain.Exercises
{
    public abstract class Exercise : Entity
    {
        public Guid UserId { get; private set; }
        public string Name { get; private set; }
        public ActivityType ActivityType { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }
        public DateTime? UpdatedAtUtc { get; private set; }
        public DateTime? DeletedAtUtc { get; private set; }

        private protected Exercise(Guid userId, string name, ActivityType activityType)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            Name = name;
            ActivityType = activityType;
            CreatedAtUtc = DateTime.UtcNow;
        }

        internal void Rename(string name)
        {
            Name = name;
            Touch();
        }

        internal void Delete()
        {
            DeletedAtUtc = DateTime.UtcNow;
        }

        private protected void Touch()
        {
            UpdatedAtUtc = DateTime.UtcNow;
        }

        private protected Exercise()
        {
            // for EF Core
            Name = null!;
        }
    }
}
