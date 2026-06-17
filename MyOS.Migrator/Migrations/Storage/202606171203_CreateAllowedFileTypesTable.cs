using FluentMigrator;

namespace MyOS.Migrator.Migrations.Storage
{
    /// <summary>
    /// Configuration table of file types allowed for upload. Read by the frontend (upload UI) and
    /// enforced again on the server. Seeded with a default set; admins may toggle is_active later.
    /// </summary>
    [Migration(202606171203)]
    public sealed class CreateAllowedFileTypesTable : Migration
    {
        // (extension, content_type, category)
        private static readonly (string Extension, string ContentType, string Category)[] DefaultTypes =
        [
            ("txt",  "text/plain",                                                                "text"),
            ("csv",  "text/csv",                                                                  "text"),
            ("md",   "text/markdown",                                                             "text"),
            ("pdf",  "application/pdf",                                                           "document"),
            ("doc",  "application/msword",                                                        "document"),
            ("docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document",   "document"),
            ("mp3",  "audio/mpeg",                                                                "audio"),
            ("wav",  "audio/wav",                                                                 "audio"),
            ("flac", "audio/flac",                                                                "audio"),
            ("ogg",  "audio/ogg",                                                                 "audio"),
            ("m4a",  "audio/mp4",                                                                 "audio"),
            ("mp4",  "video/mp4",                                                                 "video"),
            ("webm", "video/webm",                                                                "video"),
            ("mkv",  "video/x-matroska",                                                          "video"),
            ("mov",  "video/quicktime",                                                           "video"),
            ("avi",  "video/x-msvideo",                                                           "video"),
            ("zip",  "application/zip",                                                           "archive"),
            ("rar",  "application/vnd.rar",                                                        "archive"),
            ("7z",   "application/x-7z-compressed",                                                "archive"),
        ];

        public override void Up()
        {
            Create.Table("allowed_file_types")
                .InSchema("storage")

                .WithColumn("id")
                    .AsGuid()
                    .PrimaryKey()

                .WithColumn("extension")
                    .AsString(20)
                    .NotNullable()

                .WithColumn("content_type")
                    .AsString(150)
                    .NotNullable()

                .WithColumn("category")
                    .AsString(20)
                    .NotNullable()

                .WithColumn("is_active")
                    .AsBoolean()
                    .NotNullable();

            Create.Index("ix_allowed_file_types_extension")
                .OnTable("allowed_file_types").InSchema("storage")
                .OnColumn("extension").Unique();

            foreach (var (extension, contentType, category) in DefaultTypes)
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
            Delete.Table("allowed_file_types").InSchema("storage");
        }
    }
}
