using MyOS.Core.Domain.Entities;

namespace MyOS.Modules.Notes.Domain.Notes.CheckList
{
    public class CheckListItem : Entity
    {
        public Guid CheckListId { get; private set; }
        public string Text { get; private set; }
        public bool IsChecked { get; private set; }
        public int Order { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }
        public DateTime? UpdatedAtUtc { get; private set; }
        public DateTime? DeletedAtUtc { get; private set; }

        internal CheckListItem(Guid checkListId, string text, int order)
        {
            Id = Guid.NewGuid();
            CheckListId = checkListId;
            Text = text;
            IsChecked = false;
            Order = order;
            CreatedAtUtc = DateTime.UtcNow;
        }

        internal void UpdateText(string text)
        {
            Text = text;
            UpdatedAtUtc = DateTime.UtcNow;
        }

        internal void Toggle()
        {
            IsChecked = !IsChecked;
            UpdatedAtUtc = DateTime.UtcNow;
        }

        internal void SetOrder(int order)
        {
            Order = order;
            UpdatedAtUtc = DateTime.UtcNow;
        }

        internal void SoftDelete()
        {
            DeletedAtUtc = DateTime.UtcNow;
        }

        private CheckListItem()
        {
            // for EF Core
            Text = null!;
        }
    }
}
