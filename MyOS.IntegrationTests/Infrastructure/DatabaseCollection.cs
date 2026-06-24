namespace MyOS.IntegrationTests.Infrastructure
{
    // One container shared by every test in the collection (xUnit runs a collection serially,
    // so the shared SQL Server needs no per-test locking — tests isolate by a unique user id).
    [CollectionDefinition(Name)]
    public sealed class DatabaseCollection : ICollectionFixture<SqlServerFixture>
    {
        public const string Name = "Database";
    }
}
