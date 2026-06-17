namespace MyOS.Modules.Storage.Domain.AllowedFileTypes
{
    public interface IAllowedFileTypeRepository
    {
        Task<AllowedFileType?> GetByExtensionAsync(string extension, CancellationToken cancellationToken);
    }
}
