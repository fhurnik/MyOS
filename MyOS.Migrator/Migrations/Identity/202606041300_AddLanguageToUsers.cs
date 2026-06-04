using FluentMigrator;

namespace MyOS.Migrator.Migrations.Identity
{
    [Migration(202606041300)]
    public sealed class AddLanguageToUsers : Migration
    {
        public override void Up()
        {
            Alter.Table("users").InSchema("identity")
                .AddColumn("language").AsInt32().NotNullable().WithDefaultValue(0);
        }

        public override void Down()
        {
            Delete.Column("language").FromTable("users").InSchema("identity");
        }
    }
}
