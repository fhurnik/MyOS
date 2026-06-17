namespace MyOS.Modules.Storage.Application.Abstractions
{
    /// <summary>
    /// Verifies that a file's actual content (magic bytes) matches its declared extension,
    /// so a renamed/spoofed file is rejected. For extensions with no reliable signature
    /// (e.g. plain text) the implementation returns true.
    /// </summary>
    public interface IFileSignatureValidator
    {
        Task<bool> IsValidAsync(Stream content, string extension, CancellationToken cancellationToken);
    }
}
