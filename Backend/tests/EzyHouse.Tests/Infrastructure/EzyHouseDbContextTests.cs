using EzyHouse.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace EzyHouse.Tests.Infrastructure;

public class EzyHouseDbContextTests
{
    [Test]
    public void Model_can_be_created_in_sqlite()
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        var options = new DbContextOptionsBuilder<EzyHouseDbContext>()
            .UseSqlite(connection)
            .Options;

        using var db = new EzyHouseDbContext(options);
        db.Database.EnsureCreated();

        Assert.That(db.Model.GetEntityTypes().Select(entity => entity.ClrType.Name), Does.Contain("Expense"));
    }
}
