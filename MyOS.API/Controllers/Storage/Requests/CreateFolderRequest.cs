namespace MyOS.API.Controllers.Storage.Requests
{
    public sealed record CreateFolderRequest(string Name, Guid? ParentId);
}
