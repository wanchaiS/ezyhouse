using EzyHouse.Domain.Entities;
using EzyHouse.Infrastructure.Data;

namespace EzyHouse.Tests.Support;

/// <summary>Alice (primary), Bob and Cara living in one household, saved to the test database.</summary>
public sealed record TestHousehold(Household Household, User Alice, User Bob, User Cara)
{
    public IReadOnlySet<int> MemberIds => new HashSet<int> { Alice.Id, Bob.Id, Cara.Id };

    public static async Task<TestHousehold> CreateAsync(EzyHouseDbContext db, string inviteCode = "TESTHSE2")
    {
        var alice = new User { Name = "Alice", Email = $"alice.{inviteCode.ToLowerInvariant()}@example.com" };
        var bob = new User { Name = "Bob", Email = $"bob.{inviteCode.ToLowerInvariant()}@example.com" };
        var cara = new User { Name = "Cara", Email = $"cara.{inviteCode.ToLowerInvariant()}@example.com" };
        db.Users.AddRange(alice, bob, cara);
        await db.SaveChangesAsync();

        // Users first, then the household: saving both at once would be an insert cycle.
        var household = new Household { Name = "Test House", InviteCode = inviteCode, PrimaryUserId = alice.Id };
        db.Households.Add(household);
        await db.SaveChangesAsync();

        alice.HouseholdId = bob.HouseholdId = cara.HouseholdId = household.Id;
        await db.SaveChangesAsync();

        return new TestHousehold(household, alice, bob, cara);
    }
}
