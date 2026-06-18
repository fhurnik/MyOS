using MyOS.Modules.Storage.Application.Abstractions;

namespace MyOS.Modules.Storage.Infrastructure.Services
{
    /// <summary>
    /// Verifies a file's magic bytes against its extension. Extensions without a reliable
    /// signature (plain text) are not listed and pass automatically.
    /// </summary>
    internal sealed class FileSignatureValidator : IFileSignatureValidator
    {
        private readonly record struct Signature(int Offset, byte[] Magic);

        private static readonly IReadOnlyDictionary<string, Signature[]> Signatures =
            new Dictionary<string, Signature[]>
            {
                ["pdf"]  = [new(0, "%PDF"u8.ToArray())],
                ["zip"]  = [new(0, [0x50, 0x4B, 0x03, 0x04]), new(0, [0x50, 0x4B, 0x05, 0x06]), new(0, [0x50, 0x4B, 0x07, 0x08])],
                ["docx"] = [new(0, [0x50, 0x4B, 0x03, 0x04])],
                ["doc"]  = [new(0, [0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1])],
                ["rar"]  = [new(0, [0x52, 0x61, 0x72, 0x21, 0x1A, 0x07])],
                ["7z"]   = [new(0, [0x37, 0x7A, 0xBC, 0xAF, 0x27, 0x1C])],
                ["mp3"]  = [new(0, "ID3"u8.ToArray()), new(0, [0xFF, 0xFB]), new(0, [0xFF, 0xF3]), new(0, [0xFF, 0xF2]), new(0, [0xFF, 0xFA])],
                ["wav"]  = [new(0, "RIFF"u8.ToArray())],
                ["avi"]  = [new(0, "RIFF"u8.ToArray())],
                ["flac"] = [new(0, "fLaC"u8.ToArray())],
                ["ogg"]  = [new(0, "OggS"u8.ToArray())],
                ["mp4"]  = [new(4, "ftyp"u8.ToArray())],
                ["mov"]  = [new(4, "ftyp"u8.ToArray())],
                ["m4a"]  = [new(4, "ftyp"u8.ToArray())],
                ["webm"] = [new(0, [0x1A, 0x45, 0xDF, 0xA3])],
                ["mkv"]  = [new(0, [0x1A, 0x45, 0xDF, 0xA3])],
                ["jpg"]  = [new(0, [0xFF, 0xD8, 0xFF])],
                ["jpeg"] = [new(0, [0xFF, 0xD8, 0xFF])],
                ["png"]  = [new(0, [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A])],
                ["gif"]  = [new(0, "GIF8"u8.ToArray())],
                ["webp"] = [new(8, "WEBP"u8.ToArray())],
                ["bmp"]  = [new(0, "BM"u8.ToArray())],
            };

        public async Task<bool> IsValidAsync(Stream content, string extension, CancellationToken cancellationToken)
        {
            if (!Signatures.TryGetValue(extension, out var candidates))
                return true;

            var maxNeeded = candidates.Max(c => c.Offset + c.Magic.Length);
            var buffer = new byte[maxNeeded];
            var read = await content.ReadAtLeastAsync(buffer, maxNeeded, throwOnEndOfStream: false, cancellationToken);

            return candidates.Any(c => Matches(buffer, read, c));
        }

        private static bool Matches(byte[] buffer, int read, Signature signature)
        {
            if (read < signature.Offset + signature.Magic.Length)
                return false;

            return buffer.AsSpan(signature.Offset, signature.Magic.Length).SequenceEqual(signature.Magic);
        }
    }
}
