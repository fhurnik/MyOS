namespace MyOS.Modules.Notes.Domain.Notes.CheckList
{
    public interface ICheckListRepository
    {
        Task<CheckList?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task AddAsync(CheckList checkList, CancellationToken cancellationToken);
        Task AddItemAsync(CheckListItem item, CancellationToken cancellationToken);
    }
}
