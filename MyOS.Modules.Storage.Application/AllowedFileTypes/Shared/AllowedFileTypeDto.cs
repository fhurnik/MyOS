namespace MyOS.Modules.Storage.Application.AllowedFileTypes.Shared
{
    public sealed record AllowedFileTypeDto
    {
        public string Extension { get; init; } = string.Empty;
        public string ContentType { get; init; } = string.Empty;
        public string Category { get; init; } = string.Empty;
    }
}
