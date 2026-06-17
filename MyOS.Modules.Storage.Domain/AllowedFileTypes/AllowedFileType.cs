using MyOS.Core.Domain.Entities;

namespace MyOS.Modules.Storage.Domain.AllowedFileTypes
{
    /// <summary>
    /// Configuration row describing a file type permitted for upload. Seeded via migration and
    /// read on the command side to validate uploads (extension + canonical content type).
    /// </summary>
    public class AllowedFileType : Entity
    {
        public string Extension { get; private set; }
        public string ContentType { get; private set; }
        public string Category { get; private set; }
        public bool IsActive { get; private set; }

        private AllowedFileType()
        {
            // for EF Core
            Extension = null!;
            ContentType = null!;
            Category = null!;
        }
    }
}
