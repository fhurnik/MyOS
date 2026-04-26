using FluentMigrator;

namespace MyOS.Migrator.Migrations.Identity
{
    [Migration(202604261256)]
    public sealed class CreateUsersTable : Migration
    {
        public override void Up()
        {
            Create.Table("users")
                .InSchema("identity")

                .WithColumn("id")
                    .AsGuid()
                    .PrimaryKey()

                .WithColumn("email")
                    .AsString(255)
                    .NotNullable()

                .WithColumn("password_hash")
                    .AsString(500)
                    .NotNullable()

                .WithColumn("created_at_utc")
                    .AsDateTime2()
                    .NotNullable()

                .WithColumn("deleted_at_utc")
                    .AsDateTime2()
                    .Nullable();

            Create.Index("ix_users_email")
                .OnTable("users")
                .InSchema("identity")
                .OnColumn("email")
                .Ascending()
                .WithOptions()
                .Unique();
        }

        public override void Down()
        {
            Delete.Table("users")
                .InSchema("identity");
        }
    }
}
