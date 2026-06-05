using MyOS.Core.Domain.Entities;

namespace MyOS.Modules.Notes.Domain.Notes.TextNotes
{
    public class TextNote : Entity
    {
        public Guid UserId { get; private set; }
        public string Title { get; private set; }
        public string Text { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }
        public DateTime? UpdatedAtUtc { get; private set; }
        public DateTime? DeletedAtUtc { get; private set; }

        public static TextNote Create(Guid userId, string title, string text)
        {
            return new TextNote(userId, title, text);
        }

        internal TextNote(Guid userId, string title, string text)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            Title = title;
            Text = text;
            CreatedAtUtc = DateTime.UtcNow;
        }

        internal void Update(string title, string text)
        {
            Title = title;
            Text = text;
            UpdatedAtUtc = DateTime.UtcNow;
        }

        internal void Delete()
        {
            DeletedAtUtc = DateTime.UtcNow;
        }

        private TextNote()
        {
            // for EF Core
            Title = null!;
            Text = null!;
        }
    }
}
