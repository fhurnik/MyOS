using FluentMigrator;

namespace MyOS.Migrator.Migrations.system
{
    [Migration(2026042601, "Create sql_file_history table for SQL view synchronization")]
    public class CreateSqlFileHistoryTable : Migration
    {
        public override void Up()
        {
            if (!Schema.Schema("system").Exists())
                Create.Schema("system");

            Create.Table("sql_file_history").InSchema("system")
                .WithColumn("id").AsInt64().PrimaryKey().Identity()
                .WithColumn("file_name").AsString(260).NotNullable().Unique()
                .WithColumn("hash").AsString(64).NotNullable()
                .WithColumn("applied_at_utc").AsDateTime2().NotNullable();
        }

        public override void Down()
        {
            Delete.Table("sql_file_history").InSchema("system");
        }
    }
}