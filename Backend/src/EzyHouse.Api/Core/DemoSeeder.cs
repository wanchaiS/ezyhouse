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

        GroceryExpense Groceries(string description, decimal amount, int daysAgo, User payer, SplitType splitType, List<Share> shares) => new()
        {
            HouseholdId = householdId,
            Description = description,
            Amount = amount,
            Date = today.AddDays(-daysAgo),
            PaidByUserId = payer.Id,
            CreatedByUserId = payer.Id,
            SplitType = splitType,
            Shares = shares,
        };

        db.Expenses.AddRange(
            Groceries("Weekly groceries", 90.00m, 10, alice, SplitType.Equal, EqualShares(90.00m)),
            Groceries("Cleaning supplies", 24.60m, 6, bob, SplitType.Equal, EqualShares(24.60m)),
            Groceries("Pizza night", 45.00m, 3, cara, SplitType.Exact, exact.Split(45.00m,
            [
                new ShareInput(alice.Id, 15.00m),
                new ShareInput(bob.Id, 20.00m),
                new ShareInput(cara.Id, 10.00m),
            ]).ToList()));

        // Furniture is a plain expense the primary adds, whoever paid for it.
        db.Expenses.AddRange(
            new FurnitureExpense
            {
                HouseholdId = householdId,
                Description = "Couch",
                Amount = 600.00m,
                Date = today.AddDays(-120),
                PaidByUserId = alice.Id,
                CreatedByUserId = alice.Id,
                SplitType = SplitType.Equal,
                Shares = EqualShares(600.00m),
            },
            new FurnitureExpense
            {
                HouseholdId = householdId,
                Description = "TV",
                Amount = 450.00m,
                Date = today.AddDays(-90),
                PaidByUserId = bob.Id,
                CreatedByUserId = alice.Id,
                SplitType = SplitType.Equal,
                Shares = EqualShares(450.00m),
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

        var rent = new RentSchedule
        {
            HouseholdId = householdId,
            Frequency = Frequency.Weekly,
            Amount = 750.00m,
            StartDate = today.AddDays(-42),
            SplitType = SplitType.Equal,
            Shares = EqualShares(750.00m),
        };
        var electricity = new BillType
        {
            HouseholdId = householdId,
            Name = "Electricity",
            Frequency = Frequency.Quarterly,
            StartDate = today.AddDays(-150),
            Members = [alice, bob, cara],
        };
        var internet = new BillType
        {
            HouseholdId = householdId,
            Name = "Internet",
            Frequency = Frequency.Monthly,
            StartDate = today.AddDays(-75),
            Members = [alice, bob, cara],
        };
        db.RentSchedules.Add(rent);
        db.BillTypes.AddRange(electricity, internet);
        await db.SaveChangesAsync();

        // The schedules are seeded as if they had run since their start: every past date's expense
        // exists, paid by Alice, and NextDueDate is already past today.
        rent.NextDueDate = rent.StartDate;
        db.Expenses.AddRange(rent.TakeDueOccurrences(today, alice.Id));

        // Past bills have their amounts entered; the latest of each type is still at $0, waiting for the primary.
        decimal[] electricityAmounts = [312.45m];
        decimal[] internetAmounts = [79.00m, 79.00m];
        foreach ((BillType billType, decimal[] amounts) in new[] { (electricity, electricityAmounts), (internet, internetAmounts) })
        {
            billType.NextDueDate = billType.StartDate;
            IReadOnlyList<BillExpense> bills = billType.TakeDueOccurrences(today, alice.Id);
            foreach ((BillExpense bill, decimal amount) in bills.Zip(amounts))
            {
                bill.Amount = amount;
                bill.Shares = EqualShares(amount);
            }

            db.Expenses.AddRange(bills);
        }

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
