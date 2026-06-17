using MyOS.Core.Domain.Entities;

namespace MyOS.Modules.Storage.Domain.Folders
{
    public class Folder : Entity
    {
        public Guid UserId { get; private set; }
        public Guid? ParentId { get; private set; }
        public string Name { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }
        public DateTime? UpdatedAtUtc { get; private set; }
        public DateTime? DeletedAtUtc { get; private set; }

        public static Folder Create(Guid userId, string name, Guid? parentId) =>
            new(userId, name, parentId);

        internal Folder(Guid userId, string name, Guid? parentId)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            Name = name;
            ParentId = parentId;
            CreatedAtUtc = DateTime.UtcNow;
        }

        internal void Rename(string name)
        {
            Name = name;
            UpdatedAtUtc = DateTime.UtcNow;
        }

        internal void MoveTo(Guid? parentId)
        {
            ParentId = parentId;
            UpdatedAtUtc = DateTime.UtcNow;
        }

        internal void Delete()
        {
            DeletedAtUtc = DateTime.UtcNow;
        }

        private Folder()
        {
            // for EF Core
            Name = null!;
        }
    }
}
