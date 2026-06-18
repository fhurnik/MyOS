using FluentMigrator;

namespace MyOS.Migrator.Migrations.Storage
{
    /// <summary>
    /// Adds image file types to the upload allow-list (category "image").
    /// </summary>
    [Migration(202606181000)]
    public sealed class AddImageFileTypes : Migration
    {
        // (extension, content_type, category)
        private static readonly (string Extension, string ContentType, string Category)[] ImageTypes =
        [
            ("jpg",  "image/jpeg",    "image"),
            ("jpeg", "image/jpeg",    "image"),
            ("png",  "image/png",     "image"),
            ("gif",  "image/gif",     "image"),
            ("webp", "image/webp",    "image"),
            ("bmp",  "image/bmp",     "image"),
            ("svg",  "image/svg+xml", "image"),
        ];

        public override void Up()
        {
            foreach (var (extension, contentType, category) in ImageTypes)
            {
                Insert.IntoTable("allowed_file_types").InSchema("storage").Row(new
                {
                    id = Guid.NewGuid(),
                    extension,
                    content_type = contentType,
                    category,
                    is_active = true
                });
            }
        }

        public override void Down()
        {
            foreach (var (extension, _, _) in ImageTypes)
            {
                Delete.FromTable("allowed_file_types").InSchema("storage").Row(new { extension });
            }
        }
    }
}
