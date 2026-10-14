using EzyHouse.Api.Features.Bonds;
using EzyHouse.Domain.Entities;
using EzyHouse.Domain.Exceptions;
using EzyHouse.Infrastructure.Data;
using EzyHouse.Tests.Support;

namespace EzyHouse.Tests.Features;

public class BondServiceTests
{
    private static readonly DateOnly Day = new(2026, 2, 1);

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

    private BondService ServiceFor(User user) => new(new BondRepository(db), TestUsers.SignIn(db, user));

    private static BondInput Input(decimal total, params (int UserId, decimal Amount)[] contributions) =>
        new(total, Day, contributions.Select(c => new ContributionInput(c.UserId, c.Amount, Day)).ToList());

    [Test]
    public void Contributions_adding_up_to_more_than_the_total_are_rejected()
    {
        Assert.ThrowsAsync<ValidationFailedException>(() =>
            ServiceFor(house.Alice).CreateAsync(Input(1000m, (house.Alice.Id, 600m), (house.Bob.Id, 500m))));
    }

    [Test]
    public void A_member_who_is_not_the_primary_cannot_create_a_bond()
    {
        Assert.ThrowsAsync<ForbiddenException>(() =>
            ServiceFor(house.Bob).CreateAsync(Input(1000m, (house.Bob.Id, 100m))));
    }

    [Test]
    public async Task A_contribution_from_someone_outside_the_household_is_rejected()
    {
        var other = await TestHousehold.CreateAsync(db, "OTHRHSE3");

        Assert.ThrowsAsync<ValidationFailedException>(() =>
            ServiceFor(house.Alice).CreateAsync(Input(1000m, (other.Bob.Id, 100m))));
    }

    [Test]
    public async Task Updating_replaces_the_contributions_and_recalculates_what_is_outstanding()
    {
        var service = ServiceFor(house.Alice);
        BondDto created = await service.CreateAsync(Input(2000m, (house.Alice.Id, 500m), (house.Bob.Id, 500m)));

        BondDto updated = await service.UpdateAsync(created.Id, Input(2400m, (house.Cara.Id, 700m)));

        Assert.That(updated.Contributions.Select(c => c.User.Id), Is.EqualTo(new[] { house.Cara.Id }));
        Assert.That(updated.Outstanding, Is.EqualTo(1700m));
        Assert.That(db.BondContributions.Count(), Is.EqualTo(1));
        Assert.That((await ServiceFor(house.Bob).GetAsync(created.Id)).Outstanding, Is.EqualTo(1700m));
    }

    [Test]
    public async Task A_bond_from_another_household_is_not_found()
    {
        var other = await TestHousehold.CreateAsync(db, "OTHRHSE3");
        BondDto theirs = await ServiceFor(other.Alice).CreateAsync(Input(1000m));

        Assert.ThrowsAsync<NotFoundException>(() => ServiceFor(house.Alice).GetAsync(theirs.Id));
        Assert.ThrowsAsync<NotFoundException>(() => ServiceFor(house.Alice).DeleteAsync(theirs.Id));
    }

    [Test]
    public async Task The_primary_can_delete_a_bond()
    {
        var service = ServiceFor(house.Alice);
        BondDto created = await service.CreateAsync(Input(1000m, (house.Bob.Id, 100m)));

        await service.DeleteAsync(created.Id);

        Assert.That(await service.ListAsync(), Is.Empty);
    }
}
