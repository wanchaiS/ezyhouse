using EzyHouse.Api.Features.Expenses;
using EzyHouse.Domain.Entities;
using EzyHouse.Domain.Enums;
using EzyHouse.Domain.Exceptions;
using EzyHouse.Domain.Splitting;
using EzyHouse.Domain.ValueObjects;
using EzyHouse.Infrastructure.Data;
using EzyHouse.Tests.Support;

namespace EzyHouse.Tests.Features;

public class ExpenseServiceTests
{
    private TestDb testDb = null!;
    private EzyHouseDbContext db = null!;
    private TestHousehold house = null!;

    [SetUp]
    public async Task SetUp()
    {
        testDb = await TestDb.CreateAsync();
        db = testDb.NewContext();
        house = await TestHousehold.CreateAsync(db);
    }

    [TearDown]
    public async Task TearDown()
    {
        await db.DisposeAsync();
        await testDb.DisposeAsync();
    }

    [Test]
    public async Task An_equal_split_of_100_dollars_stores_33_34_33_33_33_33()
    {
        ExpenseDto created = await ServiceFor(house.Alice).CreateAsync(EqualInput(100m, house.Alice.Id));

        Assert.That(created.Shares.Select(share => share.Amount), Is.EqualTo(new[] { 33.34m, 33.33m, 33.33m }));
        Assert.That(created.CreatedBy.Id, Is.EqualTo(house.Alice.Id));
    }

    [Test]
    public async Task A_payer_who_is_not_a_member_is_rejected()
    {
        TestHousehold other = await TestHousehold.CreateAsync(db, "OTHRHSE3");

        Assert.ThrowsAsync<ValidationFailedException>(() =>
            ServiceFor(house.Alice).CreateAsync(EqualInput(100m, other.Alice.Id)));
    }

    [Test]
    public async Task Another_households_expense_is_not_found()
    {
        TestHousehold other = await TestHousehold.CreateAsync(db, "OTHRHSE3");
        ExpenseDto theirs = await ServiceFor(other.Alice).CreateAsync(
            new ExpenseInput("Their milk", 30m, new DateOnly(2026, 10, 1), other.Alice.Id, SplitType.Equal,
                [new ShareInput(other.Alice.Id, null), new ShareInput(other.Bob.Id, null)]));

        ExpenseService service = ServiceFor(house.Alice);
        Assert.ThrowsAsync<NotFoundException>(() => service.GetAsync(theirs.Id));
        Assert.ThrowsAsync<NotFoundException>(() => service.DeleteAsync(theirs.Id));
        Assert.That(await service.ListAsync(), Is.Empty);
    }

    [Test]
    public async Task Only_the_creator_can_edit_or_delete_an_expense()
    {
        ExpenseDto created = await ServiceFor(house.Alice).CreateAsync(EqualInput(100m, house.Alice.Id));

        ExpenseService bob = ServiceFor(house.Bob);
        Assert.ThrowsAsync<ForbiddenException>(() => bob.UpdateAsync(created.Id, EqualInput(50m, house.Bob.Id)));
        Assert.ThrowsAsync<ForbiddenException>(() => bob.DeleteAsync(created.Id));
    }

    [Test]
    public async Task Editing_replaces_every_field_and_recalculates_the_shares()
    {
        ExpenseService alice = ServiceFor(house.Alice);
        ExpenseDto created = await alice.CreateAsync(EqualInput(100m, house.Alice.Id));

        var input = new ExpenseInput("Pizza", 40m, new DateOnly(2026, 10, 2), house.Bob.Id, SplitType.Exact,
            [new ShareInput(house.Bob.Id, 15m), new ShareInput(house.Cara.Id, 25m)]);
        ExpenseDto updated = await alice.UpdateAsync(created.Id, input);

        Assert.That(updated.Description, Is.EqualTo("Pizza"));
        Assert.That(updated.Amount, Is.EqualTo(40m));
        Assert.That(updated.PaidBy.Id, Is.EqualTo(house.Bob.Id));
        Assert.That(updated.SplitType, Is.EqualTo(SplitType.Exact));
        Assert.That(updated.Shares.Select(share => (share.User.Id, share.Amount)),
            Is.EqualTo(new[] { (house.Bob.Id, 15m), (house.Cara.Id, 25m) }));
    }

    [Test]
    public async Task The_creator_can_delete_their_expense()
    {
        ExpenseService alice = ServiceFor(house.Alice);
        ExpenseDto created = await alice.CreateAsync(EqualInput(100m, house.Alice.Id));

        await alice.DeleteAsync(created.Id);

        Assert.ThrowsAsync<NotFoundException>(() => alice.GetAsync(created.Id));
    }

    [Test]
    public async Task Expenses_are_listed_newest_date_first_then_newest_id()
    {
        ExpenseService alice = ServiceFor(house.Alice);
        ExpenseDto older = await alice.CreateAsync(EqualInput(10m, house.Alice.Id, new DateOnly(2026, 9, 1)));
        ExpenseDto first = await alice.CreateAsync(EqualInput(10m, house.Alice.Id, new DateOnly(2026, 10, 1)));
        ExpenseDto second = await alice.CreateAsync(EqualInput(10m, house.Alice.Id, new DateOnly(2026, 10, 1)));

        IReadOnlyList<ExpenseDto> list = await alice.ListAsync();

        Assert.That(list.Select(expense => expense.Id), Is.EqualTo(new[] { second.Id, first.Id, older.Id }));
    }

    private ExpenseService ServiceFor(User user) =>
        new(new ExpenseRepository(db), TestUsers.SignIn(db, user),
            new SplitCalculator([new EqualSplitStrategy(), new ExactSplitStrategy()]));

    private ExpenseInput EqualInput(decimal amount, int paidBy, DateOnly? date = null) =>
        new("Groceries", amount, date ?? new DateOnly(2026, 10, 1), paidBy, SplitType.Equal,
            [new ShareInput(house.Alice.Id, null), new ShareInput(house.Bob.Id, null), new ShareInput(house.Cara.Id, null)]);
}
