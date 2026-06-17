using FluentMigrator;

namespace MyOS.Migrator.Migrations.Storage
{
    /// <summary>
    /// Backfills a default storage quota (5 GB) for every existing user. New users get their quota
    /// via the UserRegisteredEvent handler; this covers users created before the Storage module existed.
    /// </summary>
    [Migration(202606171202)]
    public sealed class BackfillStorageQuotasForExistingUsers : Migration
    {
        // 5 GB — keep in sync with StorageQuota.DefaultMaxBytes.
        private const long DefaultMaxBytes = 5L * 1024 * 1024 * 1024;

        public override void Up()
        {
            Execute.Sql($"""
                INSERT INTO [storage].[storage_quotas] (id, user_id, max_bytes, used_bytes, created_at_utc)
                SELECT NEWID(), u.id, {DefaultMaxBytes}, 0, SYSUTCDATETIME()
                FROM [identity].[users] u
                WHERE NOT EXISTS (
                    SELECT 1 FROM [storage].[storage_quotas] q WHERE q.user_id = u.id
                );
                """);
        }

        public override void Down()
        {
            // The quotas table was empty before this migration, so a rollback clears it.
            Execute.Sql("DELETE FROM [storage].[storage_quotas];");
        }
    }
}
