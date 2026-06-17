namespace MyOS.Modules.Storage.Application.Files.Shared
{
    /// <summary>
    /// Resolved download target. Not serialized to JSON — the controller turns it into a streamed
    /// file response (<c>PhysicalFile</c> with range processing).
    /// </summary>
    public sealed record FileDownloadDto
    {
        public required string AbsolutePath { get; init; }
        public required string ContentType { get; init; }
        public required string OriginalName { get; init; }
    }
}
