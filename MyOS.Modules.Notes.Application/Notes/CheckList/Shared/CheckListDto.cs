namespace MyOS.Modules.Notes.Application.Notes.CheckList.Shared
{
    public sealed record CheckListDto(
        Guid Id,
        Guid UserId,
        string Title,
        IReadOnlyCollection<CheckListItemDto> Items,
        DateTime CreatedAtUtc,
        DateTime? UpdatedAtUtc);
}
