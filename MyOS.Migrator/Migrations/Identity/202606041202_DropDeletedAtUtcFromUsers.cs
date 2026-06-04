using FluentMigrator;

namespace MyOS.Migrator.Migrations.Identity
{
    [Migration(202606041202)]
    public sealed class DropDeletedAtUtcFromUsers : Migration
    {
        public override void Up()
        {
            Delete.Column("deleted_at_utc").FromTable("users").InSchema("identity");
        }

        public override void Down()
        {
            Alter.Table("users").InSchema("identity")
                .AddColumn("deleted_at_utc").AsDateTime2().Nullable();
        }
    }
}
