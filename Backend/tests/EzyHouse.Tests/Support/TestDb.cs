using EzyHouse.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace EzyHouse.Tests.Support;

/// <summary>
/// A fresh in-memory SQLite database for one test. Unlike EF Core's InMemory provider, SQLite
/// enforces unique indexes and foreign keys, so the database rules are tested too.
/// </summary>
public sealed class TestDb : IAsyncDisposable
{
    private readonly SqliteConnection connection;
    private readonly DbContextOptions<EzyHouseDbContext> options;

    private TestDb(SqliteConnection connection)
    {
        this.connection = connection;
        options = new DbContextOptionsBuilder<EzyHouseDbContext>().UseSqlite(connection).Options;
    }

    public static async Task<TestDb> CreateAsync()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var testDb = new TestDb(connection);
        await using EzyHouseDbContext db = testDb.NewContext();
        await db.Database.EnsureCreatedAsync();
        return testDb;
    }

    /// <summary>A new context on the same database, so a check reads what was saved, not what is tracked.</summary>
    public EzyHouseDbContext NewContext() => new(options);

    public ValueTask DisposeAsync() => connection.DisposeAsync();
}
