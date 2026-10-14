using EzyHouse.Domain.Entities;
using EzyHouse.Domain.Extensions;
using EzyHouse.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace EzyHouse.Tests.Infrastructure;

public class HouseholdScopingTests
{
    [Test]
    public async Task ForHousehold_returns_only_the_callers_household_when_run_against_a_real_database()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<EzyHouseDbContext>().UseSqlite(connection).Options;
        await using var db = new EzyHouseDbContext(options);
        await db.Database.EnsureCreatedAsync();

        var alice = new User { Name = "Alice", Email = "alice@example.com" };
        var bob = new User { Name = "Bob", Email = "bob@example.com" };
        db.Users.AddRange(alice, bob);
        await db.SaveChangesAsync();

        var aliceHouse = new Household { Name = "Alice's", InviteCode = "AAAA2222", PrimaryUserId = alice.Id };
        var bobHouse = new Household { Name = "Bob's", InviteCode = "BBBB3333", PrimaryUserId = bob.Id };
        db.Households.AddRange(aliceHouse, bobHouse);
        await db.SaveChangesAsync();

        var aliceBond = new Bond { HouseholdId = aliceHouse.Id };
        aliceBond.Update(1000m, new DateOnly(2026, 2, 1), []);
        var bobBond = new Bond { HouseholdId = bobHouse.Id };
        bobBond.Update(2000m, new DateOnly(2026, 3, 1), []);
        db.Bonds.AddRange(aliceBond, bobBond);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        List<Bond> bonds = await db.Bonds.ForHousehold(aliceHouse.Id).ToListAsync();

        Assert.That(bonds.Select(bond => bond.Total), Is.EqualTo(new[] { 1000m }));
    }
}
