using EzyHouse.Api.Features.FurnitureItems;
using EzyHouse.Domain.Entities;
using EzyHouse.Domain.Enums;
using EzyHouse.Domain.Exceptions;
using EzyHouse.Domain.Splitting;
using EzyHouse.Domain.ValueObjects;
using EzyHouse.Infrastructure.Data;
using EzyHouse.Tests.Support;
using Microsoft.EntityFrameworkCore;

namespace EzyHouse.Tests.Features;

public class FurnitureServiceTests
{
    private static readonly DateOnly Today = new(2026, 10, 15);

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

    private FurnitureService ServiceFor(User user) =>
        new(new FurnitureRepository(db), TestUsers.SignIn(db, user),
            new SplitCalculator([new EqualSplitStrategy(), new ExactSplitStrategy()]));

    private List<ShareInput> Everyone() =>
        [new(house.Alice.Id, null), new(house.Bob.Id, null), new(house.Cara.Id, null)];

    private FurnitureInput Couch(int paidBy, string name = "Couch", decimal amount = 600m) =>
        new(name, new PurchaseInput(Today, amount, paidBy, SplitType.Equal, Everyone()));

    [Test]
    public async Task Adding_an_item_records_it_as_owned_with_one_split_purchase()
    {
        FurnitureDto couch = await ServiceFor(house.Bob).CreateAsync(Couch(house.Bob.Id));

        Assert.That(couch.Status, Is.EqualTo(FurnitureStatus.Owned));
        Assert.That(couch.CreatedBy.Id, Is.EqualTo(house.Bob.Id));
        Assert.That(couch.Purchase.PaidBy.Name, Is.EqualTo("Bob"));
        Assert.That(couch.Purchase.Shares.Select(share => share.Amount), Is.All.EqualTo(200m));
        Assert.That(couch.Sale, Is.Null);
    }

    [Test]
    public async Task Selling_an_item_that_is_already_sold_is_a_conflict()
    {
        FurnitureService service = ServiceFor(house.Alice);
        FurnitureDto couch = await service.CreateAsync(Couch(house.Alice.Id));
        var sale = new SaleInput(Today, 300m, house.Cara.Id, SplitType.Equal, Everyone());

        FurnitureDto sold = await service.SellAsync(couch.Id, sale);

        Assert.That(sold.Status, Is.EqualTo(FurnitureStatus.Sold));
        Assert.That(sold.Sale!.ReceivedBy.Name, Is.EqualTo("Cara"));
        Assert.ThrowsAsync<BusinessRuleException>(() => service.SellAsync(couch.Id, sale));
    }

    [Test]
    public async Task A_zero_dollar_disposal_splits_to_zeros()
    {
        FurnitureService service = ServiceFor(house.Alice);
        FurnitureDto couch = await service.CreateAsync(Couch(house.Alice.Id));

        FurnitureDto disposed = await service.DisposeAsync(couch.Id,
            new DisposalInput(Today, 0m, house.Bob.Id, SplitType.Equal, Everyone()));

        Assert.That(disposed.Status, Is.EqualTo(FurnitureStatus.Disposed));
        Assert.That(disposed.Disposal!.Shares, Has.Count.EqualTo(3));
        Assert.That(disposed.Disposal.Shares.Select(share => share.Amount), Is.All.EqualTo(0m));
    }

    [Test]
    public async Task Editing_someone_elses_item_is_forbidden_for_a_non_primary_but_allowed_for_the_primary()
    {
        FurnitureDto couch = await ServiceFor(house.Bob).CreateAsync(Couch(house.Bob.Id));

        Assert.ThrowsAsync<ForbiddenException>(() =>
            ServiceFor(house.Cara).UpdateAsync(couch.Id, Couch(house.Bob.Id, "Sofa")));

        FurnitureDto edited = await ServiceFor(house.Alice).UpdateAsync(couch.Id, Couch(house.Bob.Id, "Sofa", 900m));

        Assert.That(edited.Name, Is.EqualTo("Sofa"));
        Assert.That(edited.Purchase.Amount, Is.EqualTo(900m));
        Assert.That(await db.FurnitureTransactions.CountAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task The_status_filter_returns_only_matching_items()
    {
        FurnitureService service = ServiceFor(house.Alice);
        FurnitureDto couch = await service.CreateAsync(Couch(house.Alice.Id));
        await service.CreateAsync(Couch(house.Alice.Id, "Table"));
        await service.SellAsync(couch.Id, new SaleInput(Today, 300m, house.Cara.Id, SplitType.Equal, Everyone()));

        IReadOnlyList<FurnitureDto> owned = await service.ListAsync(FurnitureStatus.Owned);
        IReadOnlyList<FurnitureDto> sold = await service.ListAsync(FurnitureStatus.Sold);
        IReadOnlyList<FurnitureDto> all = await service.ListAsync(null);

        Assert.That(owned.Select(item => item.Name), Is.EqualTo(new[] { "Table" }));
        Assert.That(sold.Select(item => item.Name), Is.EqualTo(new[] { "Couch" }));
        Assert.That(all, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task A_payer_who_is_not_a_current_member_is_rejected()
    {
        TestHousehold other = await TestHousehold.CreateAsync(db, "OTHRHSE3");

        Assert.ThrowsAsync<ValidationFailedException>(() =>
            ServiceFor(house.Alice).CreateAsync(Couch(other.Alice.Id)));
    }

    [Test]
    public async Task Another_households_item_is_not_found_and_deleting_removes_its_transactions()
    {
        FurnitureDto couch = await ServiceFor(house.Bob).CreateAsync(Couch(house.Bob.Id));
        TestHousehold other = await TestHousehold.CreateAsync(db, "OTHRHSE3");

        Assert.ThrowsAsync<NotFoundException>(() => ServiceFor(other.Alice).GetAsync(couch.Id));
        Assert.ThrowsAsync<NotFoundException>(() => ServiceFor(other.Alice).DeleteAsync(couch.Id));

        await ServiceFor(house.Bob).DeleteAsync(couch.Id);

        Assert.That(await db.Furniture.CountAsync(), Is.Zero);
        Assert.That(await db.FurnitureTransactions.CountAsync(), Is.Zero);
    }
}
