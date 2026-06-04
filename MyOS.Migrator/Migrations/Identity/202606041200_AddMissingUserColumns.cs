using FluentMigrator;

namespace MyOS.Migrator.Migrations.Identity
{
    [Migration(202606041200)]
    public sealed class AddMissingUserColumns : Migration
    {
        public override void Up()
        {
            Alter.Table("users").InSchema("identity")
                .AddColumn("first_name").AsString(100).NotNullable().WithDefaultValue(string.Empty);

            Alter.Table("users").InSchema("identity")
                .AddColumn("last_name").AsString(100).NotNullable().WithDefaultValue(string.Empty);

            Alter.Table("users").InSchema("identity")
                .AddColumn("is_active").AsBoolean().NotNullable().WithDefaultValue(true);

            Alter.Table("users").InSchema("identity")
                .AddColumn("updated_at_utc").AsDateTime2().Nullable();
        }

        public override void Down()
        {
            Delete.Column("first_name").FromTable("users").InSchema("identity");
            Delete.Column("last_name").FromTable("users").InSchema("identity");
            Delete.Column("is_active").FromTable("users").InSchema("identity");
            Delete.Column("updated_at_utc").FromTable("users").InSchema("identity");
        }
    }
}
