using MyOS.Modules.Storage.Application.Files;

namespace MyOS.UnitTests.Application.Storage.Files
{
    // Validator test kept because the file-name rule is a custom .Must() encoding a real
    // requirement (an upload must carry an extension) — not framework noise like NotEmpty/GreaterThan.
    public class UploadFileCommandValidatorTests
    {
        private readonly UploadFileCommandValidator _validator = new();

        private static UploadFileCommand Command(string fileName) =>
            new(Stream.Null, fileName, "image/png", SizeBytes: 100, FolderId: null);

        [Fact]
        public void Validate_FileNameWithExtension_IsValid()
        {
            var result = _validator.Validate(Command("photo.png"));

            result.IsValid.ShouldBeTrue();
        }

        [Theory]
        [InlineData("photo")]   // no extension at all
        [InlineData("archive.")] // trailing dot, Path.GetExtension returns empty
        public void Validate_FileNameWithoutExtension_IsInvalid(string fileName)
        {
            var result = _validator.Validate(Command(fileName));

            result.IsValid.ShouldBeFalse();
            result.Errors.ShouldContain(e => e.PropertyName == nameof(UploadFileCommand.FileName));
        }
    }
}
