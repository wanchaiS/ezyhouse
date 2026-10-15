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
        ExpenseDto created = await ServiceFor(house.Alice).CreateAsync(NewEqual(ExpenseTag.Groceries, 100m, house.Alice.Id));

        Assert.That(created.Shares.Select(share => share.Amount), Is.EqualTo(new[] { 33.34m, 33.33m, 33.33m }));
        Assert.That(created.CreatedBy.Id, Is.EqualTo(house.Alice.Id));
        Assert.That(created.Tag, Is.EqualTo(ExpenseTag.Groceries));
    }

    [Test]
    public async Task A_zero_dollar_expense_is_accepted()
    {
        ExpenseDto created = await ServiceFor(house.Alice).CreateAsync(NewEqual(ExpenseTag.Bills, 0m, house.Alice.Id));

        Assert.That(created.Shares.Select(share => share.Amount), Is.All.EqualTo(0m));
    }

    [Test]
    public async Task A_payer_who_is_not_a_member_is_rejected()
    {
        TestHousehold other = await TestHousehold.CreateAsync(db, "OTHRHSE3");

        Assert.ThrowsAsync<ValidationFailedException>(() =>
            ServiceFor(house.Alice).CreateAsync(NewEqual(ExpenseTag.Groceries, 100m, other.Alice.Id)));
    }

    [Test]
    public async Task Another_households_expense_is_not_found()
    {
        TestHousehold other = await TestHousehold.CreateAsync(db, "OTHRHSE3");
        ExpenseDto theirs = await ServiceFor(other.Alice).CreateAsync(
            new NewExpenseInput(ExpenseTag.Groceries, "Their milk", 30m, new DateOnly(2026, 10, 1), other.Alice.Id, SplitType.Equal,
                [new ShareInput(other.Alice.Id, null), new ShareInput(other.Bob.Id, null)]));

        ExpenseService service = ServiceFor(house.Alice);
        Assert.ThrowsAsync<NotFoundException>(() => service.GetAsync(theirs.Id));
        Assert.ThrowsAsync<NotFoundException>(() => service.DeleteAsync(theirs.Id));
        Assert.That(await service.ListAsync(null), Is.Empty);
    }

    [TestCase(ExpenseTag.Rent)]
    [TestCase(ExpenseTag.Bills)]
    [TestCase(ExpenseTag.Furniture)]
    public void A_housemate_who_is_not_primary_cannot_add_rent_bills_or_furniture(ExpenseTag tag)
    {
        Assert.ThrowsAsync<ForbiddenException>(() => ServiceFor(house.Bob).CreateAsync(NewEqual(tag, 300m, house.Bob.Id)));
    }

    [Test]
    public async Task Any_member_can_add_groceries()
    {
        ExpenseDto created = await ServiceFor(house.Bob).CreateAsync(NewEqual(ExpenseTag.Groceries, 30m, house.Bob.Id));

        Assert.That(created.CanChange, Is.True);
    }

    [Test]
    public async Task Groceries_can_only_be_changed_by_whoever_added_them_not_even_by_the_primary()
    {
        ExpenseDto bobs = await ServiceFor(house.Bob).CreateAsync(NewEqual(ExpenseTag.Groceries, 30m, house.Bob.Id));

        Assert.ThrowsAsync<ForbiddenException>(() => ServiceFor(house.Alice).UpdateAsync(bobs.Id, Equal(60m, house.Bob.Id)));
        Assert.ThrowsAsync<ForbiddenException>(() => ServiceFor(house.Cara).DeleteAsync(bobs.Id));
        ExpenseDto updated = await ServiceFor(house.Bob).UpdateAsync(bobs.Id, Equal(60m, house.Bob.Id));
        Assert.That(updated.Amount, Is.EqualTo(60m));
    }

    [Test]
    public async Task After_a_handover_the_new_primary_can_change_old_rent_and_the_old_primary_cannot()
    {
        ExpenseDto rent = await ServiceFor(house.Alice).CreateAsync(NewEqual(ExpenseTag.Rent, 900m, house.Alice.Id));
        house.Household.PrimaryUserId = house.Bob.Id;
        await db.SaveChangesAsync();

        Assert.That((await ServiceFor(house.Alice).GetAsync(rent.Id)).CanChange, Is.False);
        Assert.ThrowsAsync<ForbiddenException>(() => ServiceFor(house.Alice).UpdateAsync(rent.Id, Equal(950m, house.Alice.Id)));
        ExpenseDto updated = await ServiceFor(house.Bob).UpdateAsync(rent.Id, Equal(950m, house.Alice.Id));
        Assert.That((updated.Amount, updated.Tag, updated.CanChange), Is.EqualTo((950m, ExpenseTag.Rent, true)));
    }

    [Test]
    public async Task Can_change_is_worked_out_for_whoever_is_asking()
    {
        ExpenseDto rent = await ServiceFor(house.Alice).CreateAsync(NewEqual(ExpenseTag.Rent, 900m, house.Alice.Id));
        ExpenseDto groceries = await ServiceFor(house.Bob).CreateAsync(NewEqual(ExpenseTag.Groceries, 30m, house.Bob.Id));

        IReadOnlyList<ExpenseDto> asBob = await ServiceFor(house.Bob).ListAsync(null);
        IReadOnlyList<ExpenseDto> asAlice = await ServiceFor(house.Alice).ListAsync(null);

        Assert.That(asBob.Select(expense => (expense.Id, expense.CanChange)),
            Is.EquivalentTo(new[] { (rent.Id, false), (groceries.Id, true) }));
        Assert.That(asAlice.Select(expense => (expense.Id, expense.CanChange)),
            Is.EquivalentTo(new[] { (rent.Id, true), (groceries.Id, false) }));
    }

    [Test]
    public async Task Editing_replaces_every_field_recalculates_the_shares_and_keeps_the_tag()
    {
        ExpenseService alice = ServiceFor(house.Alice);
        ExpenseDto created = await alice.CreateAsync(NewEqual(ExpenseTag.Furniture, 100m, house.Alice.Id));

        var input = new ExpenseInput("Desk", 40m, new DateOnly(2026, 10, 2), house.Bob.Id, SplitType.Exact,
            [new ShareInput(house.Bob.Id, 15m), new ShareInput(house.Cara.Id, 25m)]);
        ExpenseDto updated = await alice.UpdateAsync(created.Id, input);

        Assert.That(updated.Tag, Is.EqualTo(ExpenseTag.Furniture));
        Assert.That(updated.Description, Is.EqualTo("Desk"));
        Assert.That(updated.Amount, Is.EqualTo(40m));
        Assert.That(updated.PaidBy.Id, Is.EqualTo(house.Bob.Id));
        Assert.That(updated.SplitType, Is.EqualTo(SplitType.Exact));
        Assert.That(updated.Shares.Select(share => (share.User.Id, share.Amount)),
            Is.EqualTo(new[] { (house.Bob.Id, 15m), (house.Cara.Id, 25m) }));
    }

    [Test]
    public async Task The_creator_can_delete_their_groceries()
    {
        ExpenseService bob = ServiceFor(house.Bob);
        ExpenseDto created = await bob.CreateAsync(NewEqual(ExpenseTag.Groceries, 100m, house.Bob.Id));

        await bob.DeleteAsync(created.Id);

        Assert.ThrowsAsync<NotFoundException>(() => bob.GetAsync(created.Id));
    }

    [Test]
    public async Task Listing_by_tag_returns_only_that_tag_and_no_tag_returns_every_one_newest_first()
    {
        ExpenseService alice = ServiceFor(house.Alice);
        ExpenseDto rent = await alice.CreateAsync(NewEqual(ExpenseTag.Rent, 10m, house.Alice.Id, new DateOnly(2026, 9, 1)));
        ExpenseDto first = await alice.CreateAsync(NewEqual(ExpenseTag.Groceries, 10m, house.Alice.Id, new DateOnly(2026, 10, 1)));
        ExpenseDto second = await alice.CreateAsync(NewEqual(ExpenseTag.Groceries, 10m, house.Alice.Id, new DateOnly(2026, 10, 1)));

        IReadOnlyList<ExpenseDto> groceries = await alice.ListAsync(ExpenseTag.Groceries);
        IReadOnlyList<ExpenseDto> everything = await alice.ListAsync(null);

        Assert.That(groceries.Select(expense => expense.Id), Is.EqualTo(new[] { second.Id, first.Id }));
        Assert.That(everything.Select(expense => expense.Id), Is.EqualTo(new[] { second.Id, first.Id, rent.Id }));
        Assert.That(await alice.ListAsync(ExpenseTag.Bills), Is.Empty);
    }

    private ExpenseService ServiceFor(User user) =>
        new(new ExpenseRepository(db), TestUsers.SignIn(db, user),
            new SplitCalculator([new EqualSplitStrategy(), new ExactSplitStrategy()]));

    private ExpenseInput Equal(decimal amount, int paidBy, DateOnly? date = null) =>
        new("Groceries", amount, date ?? new DateOnly(2026, 10, 1), paidBy, SplitType.Equal,
            [new ShareInput(house.Alice.Id, null), new ShareInput(house.Bob.Id, null), new ShareInput(house.Cara.Id, null)]);

    private NewExpenseInput NewEqual(ExpenseTag tag, decimal amount, int paidBy, DateOnly? date = null)
    {
        ExpenseInput input = Equal(amount, paidBy, date);
        return new NewExpenseInput(tag, input.Description, input.Amount, input.Date, input.PaidByUserId, input.SplitType, input.Shares);
    }
}
