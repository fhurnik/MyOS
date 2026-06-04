using FluentMigrator;

namespace MyOS.Migrator.Migrations.Identity
{
    [Migration(202606041201)]
    public sealed class CreateRefreshTokensTable : Migration
    {
        public override void Up()
        {
            Create.Table("refresh_tokens")
                .InSchema("identity")

                .WithColumn("id")
                    .AsGuid()
                    .PrimaryKey()

                .WithColumn("user_id")
                    .AsGuid()
                    .NotNullable()

                .WithColumn("token")
                    .AsString(500)
                    .NotNullable()

                .WithColumn("expires_at_utc")
                    .AsDateTime2()
                    .NotNullable()

                .WithColumn("created_at_utc")
                    .AsDateTime2()
                    .NotNullable()

                .WithColumn("revoked_at_utc")
                    .AsDateTime2()
                    .Nullable()

                .WithColumn("replaced_by_token")
                    .AsString(500)
                    .Nullable();

            Create.ForeignKey("fk_refresh_tokens_users")
                .FromTable("refresh_tokens").InSchema("identity").ForeignColumn("user_id")
                .ToTable("users").InSchema("identity").PrimaryColumn("id");

            Create.Index("ix_refresh_tokens_token")
                .OnTable("refresh_tokens").InSchema("identity")
                .OnColumn("token").Ascending()
                .WithOptions().Unique();

            Create.Index("ix_refresh_tokens_user_id")
                .OnTable("refresh_tokens").InSchema("identity")
                .OnColumn("user_id").Ascending();
        }

        public override void Down()
        {
            Delete.Table("refresh_tokens").InSchema("identity");
        }
    }
}
