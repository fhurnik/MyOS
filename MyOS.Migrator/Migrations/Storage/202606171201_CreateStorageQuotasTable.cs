using FluentMigrator;

namespace MyOS.Migrator.Migrations.Storage
{
    [Migration(202606171201)]
    public sealed class CreateStorageQuotasTable : Migration
    {
        public override void Up()
        {
            Create.Table("storage_quotas")
                .InSchema("storage")

                .WithColumn("id")
                    .AsGuid()
                    .PrimaryKey()

                .WithColumn("user_id")
                    .AsGuid()
                    .NotNullable()

                .WithColumn("max_bytes")
                    .AsInt64()
                    .NotNullable()

                .WithColumn("used_bytes")
                    .AsInt64()
                    .NotNullable()

                .WithColumn("created_at_utc")
                    .AsDateTime2()
                    .NotNullable()

                .WithColumn("updated_at_utc")
                    .AsDateTime2()
                    .Nullable();

            Create.Index("ix_storage_quotas_user_id")
                .OnTable("storage_quotas").InSchema("storage")
                .OnColumn("user_id").Unique();

            Create.ForeignKey("fk_storage_quotas_user_id")
                .FromTable("storage_quotas").InSchema("storage").ForeignColumn("user_id")
                .ToTable("users").InSchema("identity").PrimaryColumn("id");
        }

        public override void Down()
        {
            Delete.ForeignKey("fk_storage_quotas_user_id")
                .OnTable("storage_quotas").InSchema("storage");

            Delete.Table("storage_quotas").InSchema("storage");
        }
    }
}
