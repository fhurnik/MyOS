using MyOS.Core.Domain.Entities;

namespace MyOS.Modules.Notes.Domain.Notes.CheckList
{
    public class CheckList : Entity
    {
        public Guid UserId { get; private set; }
        public string Title { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }
        public DateTime? UpdatedAtUtc { get; private set; }
        public DateTime? DeletedAtUtc { get; private set; }

        private List<CheckListItem> _items = [];
        public IReadOnlyCollection<CheckListItem> Items => _items.AsReadOnly();

        public static CheckList Create(Guid userId, string title)
        {
            return new CheckList(userId, title);
        }

        internal CheckList(Guid userId, string title)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            Title = title;
            CreatedAtUtc = DateTime.UtcNow;
        }

        internal void UpdateTitle(string title)
        {
            Title = title;
            UpdatedAtUtc = DateTime.UtcNow;
        }

        internal CheckListItem AddItem(string text)
        {
            var activeItems = _items.Where(i => i.DeletedAtUtc is null).ToList();
            var order = activeItems.Count == 0 ? 1 : activeItems.Max(i => i.Order) + 1;
            var item = new CheckListItem(Id, text, order);
            _items.Add(item);
            UpdatedAtUtc = DateTime.UtcNow;
            return item;
        }

        internal bool RemoveItem(Guid itemId)
        {
            var item = _items.FirstOrDefault(i => i.Id == itemId && i.DeletedAtUtc is null);
            if (item is null)
                return false;

            item.SoftDelete();
            UpdatedAtUtc = DateTime.UtcNow;
            return true;
        }

        internal bool UpdateItem(Guid itemId, string text)
        {
            var item = _items.FirstOrDefault(i => i.Id == itemId && i.DeletedAtUtc is null);
            if (item is null)
                return false;

            item.UpdateText(text);
            UpdatedAtUtc = DateTime.UtcNow;
            return true;
        }

        internal bool ToggleItem(Guid itemId)
        {
            var item = _items.FirstOrDefault(i => i.Id == itemId && i.DeletedAtUtc is null);
            if (item is null)
                return false;

            item.Toggle();
            UpdatedAtUtc = DateTime.UtcNow;
            return true;
        }

        internal bool ReorderItem(Guid itemId, int newOrder)
        {
            var item = _items.FirstOrDefault(i => i.Id == itemId && i.DeletedAtUtc is null);
            if (item is null)
                return false;

            var activeItems = _items
                .Where(i => i.DeletedAtUtc is null)
                .OrderBy(i => i.Order)
                .ToList();

            var clamped = Math.Clamp(newOrder, 1, activeItems.Count);
            activeItems.Remove(item);
            activeItems.Insert(clamped - 1, item);

            for (var i = 0; i < activeItems.Count; i++)
                activeItems[i].SetOrder(i + 1);

            UpdatedAtUtc = DateTime.UtcNow;
            return true;
        }

        internal void Delete()
        {
            DeletedAtUtc = DateTime.UtcNow;
        }

        private CheckList()
        {
            // for EF Core
            Title = null!;
        }
    }
}
