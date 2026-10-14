using EzyHouse.Domain.Entities;
using EzyHouse.Domain.Enums;
using EzyHouse.Domain.Splitting;
using EzyHouse.Domain.ValueObjects;
using EzyHouse.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EzyHouse.Api.Core;

/// <summary>
/// Fills an empty database with one demo household so the app has something to show.
/// </summary>
public static class DemoSeeder
{
    private const string DemoPassword = "Password1!";

    public static async Task SeedAsync(EzyHouseDbContext db, IPasswordHasher<User> passwordHasher, DateOnly today)
    {
        if (await db.Users.AnyAsync())
            return;

        User alice = CreateUser("Alice", "alice@example.com", passwordHasher);
        User bob = CreateUser("Bob", "bob@example.com", passwordHasher);
        User cara = CreateUser("Cara", "cara@example.com", passwordHasher);
        db.Users.AddRange(alice, bob, cara);
        await db.SaveChangesAsync();

        // Users and the household point at each other, so they are saved in separate steps.
        var household = new Household { Name = "Demo House", InviteCode = "DEMH7KQ2", PrimaryUserId = alice.Id };
        db.Households.Add(household);
        await db.SaveChangesAsync();

        alice.HouseholdId = household.Id;
        bob.HouseholdId = household.Id;
        cara.HouseholdId = household.Id;
        await db.SaveChangesAsync();

        int householdId = household.Id;
        int[] everyone = [alice.Id, bob.Id, cara.Id];
        var equal = new EqualSplitStrategy();
        var exact = new ExactSplitStrategy();

        List<Share> EqualShares(decimal total) =>
            equal.Split(total, everyone.Select(id => new ShareInput(id, null)).ToList()).ToList();

        db.Expenses.AddRange(
            new Expense
            {
                HouseholdId = householdId,
                Description = "Weekly groceries",
                Amount = 90.00m,
                Date = today.AddDays(-10),
                PaidByUserId = alice.Id,
                CreatedByUserId = alice.Id,
                SplitType = SplitType.Equal,
                Shares = EqualShares(90.00m),
            },
            new Expense
            {
                HouseholdId = householdId,
                Description = "Cleaning supplies",
                Amount = 24.60m,
                Date = today.AddDays(-6),
                PaidByUserId = bob.Id,
                CreatedByUserId = bob.Id,
                SplitType = SplitType.Equal,
                Shares = EqualShares(24.60m),
            },
            new Expense
            {
                HouseholdId = householdId,
                Description = "Pizza night",
                Amount = 45.00m,
                Date = today.AddDays(-3),
                PaidByUserId = cara.Id,
                CreatedByUserId = cara.Id,
                SplitType = SplitType.Exact,
                Shares = exact.Split(45.00m,
                [
                    new ShareInput(alice.Id, 15.00m),
                    new ShareInput(bob.Id, 20.00m),
                    new ShareInput(cara.Id, 10.00m),
                ]).ToList(),
            });

        db.Settlements.Add(new Settlement
        {
            HouseholdId = householdId,
            FromUserId = bob.Id,
            ToUserId = alice.Id,
            Amount = 20.00m,
            Date = today.AddDays(-2),
            CreatedByUserId = bob.Id,
        });

        // Rent periods and bills are generated when their lists are read, so only the schedules are seeded.
        db.RentSchedules.Add(new RentSchedule
        {
            HouseholdId = householdId,
            Frequency = Frequency.Weekly,
            Amount = 750.00m,
            StartDate = today.AddDays(-42),
            SplitType = SplitType.Equal,
            Shares = EqualShares(750.00m),
        });

        db.BillTypes.AddRange(
            new BillType
            {
                HouseholdId = householdId,
                Name = "Electricity",
                Frequency = Frequency.Quarterly,
                StartDate = today.AddDays(-60),
                Members = [alice, bob, cara],
            },
            new BillType
            {
                HouseholdId = householdId,
                Name = "Internet",
                Frequency = Frequency.Monthly,
                StartDate = today.AddDays(-75),
                Members = [alice, bob, cara],
            });

        var couch = new Furniture
        {
            HouseholdId = householdId,
            Name = "Couch",
            CreatedByUserId = alice.Id,
            Transactions =
            [
                new FurniturePurchase
                {
                    Date = today.AddDays(-120),
                    Amount = 600.00m,
                    CounterpartyUserId = alice.Id,
                    SplitType = SplitType.Equal,
                    Shares = EqualShares(600.00m),
                },
            ],
        };
        couch.RecordSale(new FurnitureSale
        {
            Date = today.AddDays(-7),
            Amount = 300.00m,
            CounterpartyUserId = cara.Id,
            SplitType = SplitType.Equal,
            Shares = EqualShares(300.00m),
        });

        var tv = new Furniture
        {
            HouseholdId = householdId,
            Name = "TV",
            CreatedByUserId = bob.Id,
            Transactions =
            [
                new FurniturePurchase
                {
                    Date = today.AddDays(-90),
                    Amount = 450.00m,
                    CounterpartyUserId = bob.Id,
                    SplitType = SplitType.Equal,
                    Shares = EqualShares(450.00m),
                },
            ],
        };
        db.Furniture.AddRange(couch, tv);

        var bond = new Bond { HouseholdId = householdId };
        bond.Update(2400.00m, today.AddDays(-42),
        [
            new BondContribution { UserId = alice.Id, Amount = 800.00m, Date = today.AddDays(-42) },
        ]);
        db.Bonds.Add(bond);

        await db.SaveChangesAsync();
    }

    private static User CreateUser(string name, string email, IPasswordHasher<User> passwordHasher)
    {
        var user = new User { Name = name, Email = email };
        user.PasswordHash = passwordHasher.HashPassword(user, DemoPassword);
        return user;
    }
}
