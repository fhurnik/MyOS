using DomainCheckList = MyOS.Modules.Notes.Domain.Notes.CheckList.CheckList;

namespace MyOS.UnitTests.Domain.Notes.CheckList
{
    // CheckList is the aggregate root for its items. The decisions worth testing are the
    // soft-delete-aware order assignment (AddItem), the find-or-false lookups shared by
    // Remove/Update/Toggle/Reorder, and the reindexing in ReorderItem.
    public class CheckListTests
    {
        private static DomainCheckList NewList() =>
            DomainCheckList.Create(userId: Guid.NewGuid(), title: "Shopping");

        [Fact]
        public void AddItem_Twice_AssignsSequentialOrders()
        {
            var list = NewList();

            var first = list.AddItem("milk");
            var second = list.AddItem("bread");

            first.Order.ShouldBe(1);
            second.Order.ShouldBe(2);
        }

        [Fact]
        public void AddItem_AfterRemovingFirst_ContinuesFromMaxOrder()
        {
            var list = NewList();
            var a = list.AddItem("milk");   // order 1
            list.AddItem("bread");          // order 2
            list.RemoveItem(a.Id);          // soft-delete order 1

            var c = list.AddItem("eggs");

            // Order is max of NON-deleted orders + 1 (here {2} -> 3); deleted rows are ignored.
            c.Order.ShouldBe(3);
        }

        [Fact]
        public void RemoveItem_UnknownId_ReturnsFalse()
        {
            var list = NewList();
            list.AddItem("milk");

            list.RemoveItem(Guid.NewGuid()).ShouldBeFalse();
        }

        [Fact]
        public void RemoveItem_ExistingId_SoftDeletesSoItIsNoLongerActed()
        {
            var list = NewList();
            var item = list.AddItem("milk");

            list.RemoveItem(item.Id).ShouldBeTrue();

            // A second operation on the same id now fails — the item is invisible to the aggregate.
            list.ToggleItem(item.Id).ShouldBeFalse();
        }

        [Fact]
        public void ToggleItem_ExistingId_FlipsIsChecked()
        {
            var list = NewList();
            var item = list.AddItem("milk");

            list.ToggleItem(item.Id).ShouldBeTrue();
            item.IsChecked.ShouldBeTrue();

            list.ToggleItem(item.Id).ShouldBeTrue();
            item.IsChecked.ShouldBeFalse();
        }

        [Fact]
        public void UpdateItem_UnknownId_ReturnsFalse()
        {
            var list = NewList();

            list.UpdateItem(Guid.NewGuid(), "x").ShouldBeFalse();
        }

        [Fact]
        public void ReorderItem_MovingLastToFirst_ReindexesAllActiveItemsContiguously()
        {
            var list = NewList();
            var a = list.AddItem("a"); // order 1
            var b = list.AddItem("b"); // order 2
            var c = list.AddItem("c"); // order 3

            list.ReorderItem(c.Id, newOrder: 1).ShouldBeTrue();

            // c jumps to the front; everything is renumbered 1..n with no gaps.
            c.Order.ShouldBe(1);
            a.Order.ShouldBe(2);
            b.Order.ShouldBe(3);
        }

        [Fact]
        public void ReorderItem_NewOrderBeyondCount_ClampsToLastPosition()
        {
            var list = NewList();
            var a = list.AddItem("a"); // order 1
            var b = list.AddItem("b"); // order 2

            // newOrder 99 is clamped to the active item count (2), so a moves to the end.
            list.ReorderItem(a.Id, newOrder: 99).ShouldBeTrue();

            b.Order.ShouldBe(1);
            a.Order.ShouldBe(2);
        }

        [Fact]
        public void ReorderItem_UnknownId_ReturnsFalse()
        {
            var list = NewList();
            list.AddItem("a");

            list.ReorderItem(Guid.NewGuid(), newOrder: 1).ShouldBeFalse();
        }
    }
}
